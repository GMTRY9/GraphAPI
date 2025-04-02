using Microsoft.AspNetCore.Mvc;
using GraphAPI.Data;
using GraphAPI.Models;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace GraphAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class queryController : ControllerBase
    {
        private readonly GraphAPIContext _context;

        char escapeChar = '`';
        char jsonCmdChar = '#';

        public List<object> tables = new List<object>();

        public Dictionary<string, string[]> columnDescriptors = new Dictionary<string, string[]>();
        public Dictionary<string, string[]> tableDescriptors = new Dictionary<string, string[]>();

        public Dictionary<int, object> contextReferences = new Dictionary<int, object>();

        public Dictionary<int, string> sqlFunctions = new Dictionary<int, string>
        {
            { 0, ", " },
            { 1, " AND " },
            { 2, " OR " },
            { 3, " = " },
            { 4, " <> " },
            { 5, " > " },
            { 6, " < " },
            { 7, " >= " },
            { 8, " <= " },
            { 9, " LIKE " }
        };

        public queryController(GraphAPIContext context)
        {
            _context = context;

            tableDescriptors.Add("1", new string[] { "node" });
            tables.Add(new node());
            contextReferences.Add(1, _context.node);

            tableDescriptors.Add("2", new string[] { "nodetype" });
            tables.Add(new nodetype());
            contextReferences.Add(2, _context.nodetype);

            tableDescriptors.Add("3", new string[] { "edge" });
            tables.Add(new edge());
            contextReferences.Add(3, _context.edge);

            tableDescriptors.Add("4", new string[] { "edgetype" });
            tables.Add(new edgetype());
            contextReferences.Add(4, _context.edgetype);

            tableDescriptors.Add("5", new string[] { "graph" });
            tables.Add(new graph());
            contextReferences.Add(5, _context.graph);

            int columnid = 0;

            foreach (var table in tables)
            {
                string tablename = table.GetType().Name;
                foreach (var property in table.GetType().GetProperties())
                {
                    columnid += 1;
                    string attributename = property.ToString().Split(' ').Last();
                    string[] attributeparts = new string[] { tablename, attributename };
                    columnDescriptors.Add(ToAlpha(columnid), attributeparts);
                }
            }
        }

        // POST: api/query
        [HttpPost]
        public async Task<ActionResult<object>> Postquery(DQuery query)
        {
            if ((query.from < 1) || (query.select.Count() == 0) || (query.where.Count() == 0)) {
                return BadRequest();
            }

            var tableContext = contextReferences[query.from];

            List<string> selectedColumns = new List<string>();
            foreach (char column in query.select)
            {
                selectedColumns.Add((string)columnDescriptors[column.ToString()].GetValue(1));
            }

            string whereClause = "";
            bool escaped = false;
            bool jsonCmdExpected = false;

            foreach (char element in query.where)
            {
                if (element == escapeChar)
                {
                    escaped = !escaped;
                    continue;
                }
                else if (element == jsonCmdChar)
                {
                    jsonCmdExpected = true;
                }
                else if (Char.IsDigit(element) && !escaped && !jsonCmdExpected)
                {
                    whereClause += sqlFunctions[(int)element - '0'];
                }
                else if (!Char.IsDigit(element) && !escaped && !jsonCmdExpected)
                {
                    whereClause += (string)columnDescriptors[element.ToString()].GetValue(1);
                }
                else if (escaped)
                {
                    whereClause += element.ToString();
                }
                else if (jsonCmdExpected)
                {
                    jsonCmdExpected = false;
                }
            }

            Type tableType = contextReferences[query.from].GetType().GetGenericArguments()[0];

            var buildQueryMethod = typeof(queryController).GetMethod(nameof(BuildQuery));

            var genericBuildQueryMethod = buildQueryMethod.MakeGenericMethod(tableType);

            var queryResult = genericBuildQueryMethod.Invoke(null, new object[] { tableContext, selectedColumns, whereClause });
            return Ok(queryResult);
        }

        // GET: api/query/columnids
        [HttpGet("getcolumnids")]
        public async Task<ActionResult<Dictionary<string, Dictionary<string, string>>>> GetColumnIds()
        {
            return Ok(new Dictionary<string, Dictionary<string, string[]>>
            {
                { "columnDescriptors", columnDescriptors },

                { "tableDescriptors", tableDescriptors }
            });
        }

        public static IQueryable<dynamic> BuildQuery<TSource>(IQueryable<TSource> tableContext, List<string> columns, string condition)
        {
            var parameter = Expression.Parameter(typeof(TSource), "x");

            // Dynamically create a list of property expressions for each selected column
            var selectedProperties = columns.Select(col =>
            {
                return Expression.Property(parameter, col);
            }).ToArray();

            var lambda = DynamicExpressionParser.ParseLambda(new[] { parameter }, null, condition);

            var filteredQuery = tableContext.Where(lambda);

            // Create a dictionary of property names and their types
            var properties = columns.ToDictionary(col => col, col => typeof(TSource).GetProperty(col).PropertyType);

            // Create a dynamic type with selected properties
            var dynamicType = DynamicClassFactory2.CreateType(properties);

            // Create bindings for the dynamic type
            var bindings = selectedProperties.Select((property, index) =>
            {
                var propertyName = columns[index];
                return Expression.Bind(
                    dynamicType.GetProperty(propertyName),
                    property
                );
            }).ToArray();

            // Create a new object (dynamic type) with selected properties
            var anonymousTypeExpression = Expression.New(dynamicType);

            var projection = Expression.MemberInit(anonymousTypeExpression, bindings);

            // Create the lambda to select the new dynamic object
            var columnSelector = Expression.Lambda(projection, parameter);

            // Convert the columnSelector expression to a string
            var columnSelectorString = string.Join(", ", columns.Select(col => $"x.{col}"));

            // Apply the select projection
            var selectedQuery = filteredQuery.Select($"new({columnSelectorString})");

            return selectedQuery.Cast<dynamic>();
        }


        private static string ToAlpha(int i)
        {
            string result = "";
            do
            {
                result = (char)((i - 1) % 26 + 'A') + result;
                i = (i - 1) / 26;
            } while (i > 0);
            return result;
        }
    }
}