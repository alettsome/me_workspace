#!/usr/bin/env node

const { Server } = require('@modelcontextprotocol/sdk/server/index.js');
const { StdioServerTransport } = require('@modelcontextprotocol/sdk/server/stdio.js');
const { CallToolRequestSchema, ListToolsRequestSchema } = require('@modelcontextprotocol/sdk/types.js');
const sqlite3 = require('sqlite3').verbose();
const path = require('path');

const DB_PATH = path.join(__dirname, 'App_Data', 'me_workspace.db');

// Create MCP server
const server = new Server(
  {
    name: 'me_workspace-sqlite',
    version: '1.0.0',
  },
  {
    capabilities: {
      tools: {},
    },
  }
);

// Helper to run SQL queries
function runQuery(sql, params = []) {
  return new Promise((resolve, reject) => {
    const db = new sqlite3.Database(DB_PATH, sqlite3.OPEN_READONLY);
    db.all(sql, params, (err, rows) => {
      db.close();
      if (err) reject(err);
      else resolve(rows);
    });
  });
}

// List available tools
server.setRequestHandler(ListToolsRequestSchema, async () => {
  return {
    tools: [
      {
        name: 'query_sources',
        description: 'List all sources (PDFs, documents) in the database',
        inputSchema: {
          type: 'object',
          properties: {},
        },
      },
      {
        name: 'query_chunks',
        description: 'Search chunks by content or get chunks for a specific source',
        inputSchema: {
          type: 'object',
          properties: {
            search: {
              type: 'string',
              description: 'Search text to find in chunks (optional)',
            },
            source_id: {
              type: 'string',
              description: 'Filter chunks by source ID (optional)',
            },
            limit: {
              type: 'number',
              description: 'Maximum number of results (default: 50)',
              default: 50,
            },
          },
        },
      },
      {
        name: 'get_source_details',
        description: 'Get detailed information about a specific source',
        inputSchema: {
          type: 'object',
          properties: {
            source_id: {
              type: 'string',
              description: 'The ID of the source',
            },
          },
          required: ['source_id'],
        },
      },
      {
        name: 'query_database',
        description: 'Execute a custom SQL SELECT query on the database',
        inputSchema: {
          type: 'object',
          properties: {
            sql: {
              type: 'string',
              description: 'SQL SELECT query to execute',
            },
          },
          required: ['sql'],
        },
      },
    ],
  };
});

// Handle tool calls
server.setRequestHandler(CallToolRequestSchema, async (request) => {
  const { name, arguments: args } = request.params;

  try {
    switch (name) {
      case 'query_sources': {
        const sources = await runQuery(
          'SELECT Id, FileName, ProjectId, FileHash, TotalChunks, TotalCharacters, ProcessedUtc, Status FROM Sources ORDER BY ProcessedUtc DESC'
        );
        return {
          content: [
            {
              type: 'text',
              text: JSON.stringify(sources, null, 2),
            },
          ],
        };
      }

      case 'query_chunks': {
        let sql = 'SELECT * FROM Chunks';
        const conditions = [];
        const params = [];

        if (args.source_id) {
          conditions.push('SourceId = ?');
          params.push(args.source_id);
        }

        if (args.search) {
          conditions.push('Content LIKE ?');
          params.push(`%${args.search}%`);
        }

        if (conditions.length > 0) {
          sql += ' WHERE ' + conditions.join(' AND ');
        }

        sql += ` ORDER BY ChunkIndex LIMIT ${args.limit || 50}`;

        const chunks = await runQuery(sql, params);
        return {
          content: [
            {
              type: 'text',
              text: JSON.stringify(chunks, null, 2),
            },
          ],
        };
      }

      case 'get_source_details': {
        const source = await runQuery('SELECT * FROM Sources WHERE Id = ?', [args.source_id]);
        const chunks = await runQuery('SELECT COUNT(*) as count FROM Chunks WHERE SourceId = ?', [args.source_id]);
        
        return {
          content: [
            {
              type: 'text',
              text: JSON.stringify({
                source: source[0],
                chunk_count: chunks[0]?.count || 0,
              }, null, 2),
            },
          ],
        };
      }

      case 'query_database': {
        // Security: Only allow SELECT queries
        if (!args.sql.trim().toLowerCase().startsWith('select')) {
          throw new Error('Only SELECT queries are allowed');
        }

        const results = await runQuery(args.sql);
        return {
          content: [
            {
              type: 'text',
              text: JSON.stringify(results, null, 2),
            },
          ],
        };
      }

      default:
        throw new Error(`Unknown tool: ${name}`);
    }
  } catch (error) {
    return {
      content: [
        {
          type: 'text',
          text: `Error: ${error.message}`,
        },
      ],
      isError: true,
    };
  }
});

// Start server
async function main() {
  const transport = new StdioServerTransport();
  await server.connect(transport);
  console.error('MCP SQLite server running');
}

main().catch((error) => {
  console.error('Server error:', error);
  process.exit(1);
});
