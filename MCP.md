# MCP (Model Context Protocol) Setup

This repository includes configuration for [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) servers, enabling AI tooling integration for Umbraco CMS development workflows.

## Overview

MCP allows AI assistants (like Claude) to interact with external tools and services. This repository configures two MCP servers:

| Server | Purpose | Package |
|--------|---------|---------|
| **umbraco-cms** | Manage Umbraco content types, documents, and media | `@umbraco-cms/mcp-dev@17` |
| **playwright** | Browser automation for testing and debugging | `@playwright/mcp@latest` |

## Quick Start

### 1. Start Umbraco Locally

Ensure your local Umbraco instance is running at `https://localhost:44339` (or update the URL in your `.env.local`).

### 2. Configure Environment Variables

Copy the example environment file and customize it:

```bash
cp .env.example .env.local
```

Edit `.env.local` with your local settings:

```env
UMBRACO_CLIENT_ID=umbraco-back-office-mcp
UMBRACO_CLIENT_SECRET=<your-client-secret>
UMBRACO_BASE_URL=https://localhost:44339
NODE_TLS_REJECT_UNAUTHORIZED=0
UMBRACO_INCLUDE_TOOL_COLLECTIONS=data-type,document-type,document,media-type,media
```

### 3. Configure the OAuth Client in Umbraco

Create an OAuth client in your Umbraco instance with:
- **Client ID**: `umbraco-back-office-mcp`
- **Client Secret**: The value you set in `.env.local`
- **Grant Type**: Client Credentials

## Environment Variables Reference

| Variable | Description | Example |
|----------|-------------|---------|
| `UMBRACO_CLIENT_ID` | OAuth client ID configured in Umbraco | `umbraco-back-office-mcp` |
| `UMBRACO_CLIENT_SECRET` | OAuth client secret (keep secure!) | `your-secure-secret` |
| `UMBRACO_BASE_URL` | URL of your local Umbraco instance | `https://localhost:44339` |
| `NODE_TLS_REJECT_UNAUTHORIZED` | Set to `0` for self-signed certificates (local dev only) | `0` |
| `UMBRACO_INCLUDE_TOOL_COLLECTIONS` | Comma-separated list of tool collections to enable | `data-type,document-type,document` |

### Tool Collections

The `UMBRACO_INCLUDE_TOOL_COLLECTIONS` variable controls which Umbraco MCP tools are available:

- `data-type` - Manage data types (property editors)
- `document-type` - Manage document types (content types)
- `document` - Manage content/documents
- `media-type` - Manage media types
- `media` - Manage media items

## Security Considerations

> **Warning**: This configuration is for **local development only**.

### Self-Signed Certificates

`NODE_TLS_REJECT_UNAUTHORIZED=0` disables SSL certificate validation. This is necessary for self-signed certificates in local development but:

- **Never use in production**
- Affects all HTTPS connections made by Node.js processes
- Consider trusting your local development certificate instead

### Client Secrets

- Never commit real secrets to source control
- The `.env.local` file is gitignored for this reason
- Use strong, unique secrets even in development
- The example value `1234567890` in `.env.example` is a placeholder only

## File Structure

```
Umbraco-CMS/
├── .mcp.json              # MCP server configuration
├── .env.example           # Example environment variables (committed)
├── .env.local             # Your local environment variables (gitignored)
├── .claude/
│   ├── settings.json      # Shared Claude AI permissions (committed)
│   └── settings.local.json # Local Claude overrides (gitignored)
└── .gitignore             # Ignores .env.local and settings.local.json
```

## Claude AI Permissions

The `.claude/settings.json` file configures which MCP tools Claude can use automatically without prompting. This is shared across the team for consistent developer experience.

### Customizing Permissions Locally

Create `.claude/settings.local.json` to override permissions for your environment:

```json
{
  "permissions": {
    "allow": [
      "mcp__umbraco__get-all-document-types"
    ]
  }
}
```

## Troubleshooting

### "Connection refused" errors

- Ensure Umbraco is running at the configured `UMBRACO_BASE_URL`
- Check that the port matches your local setup

### "Unauthorized" errors

- Verify the OAuth client is configured in Umbraco
- Check that `UMBRACO_CLIENT_ID` and `UMBRACO_CLIENT_SECRET` match
- Ensure the client has appropriate permissions

### "Certificate" errors

- For local development, set `NODE_TLS_REJECT_UNAUTHORIZED=0` in `.env.local`
- Alternatively, trust your local development certificate

### MCP server not starting

- Ensure Node.js is installed (v18+ recommended)
- Run `npx @umbraco-cms/mcp-dev@17 --help` to verify the package works

## Further Reading

- [Model Context Protocol Documentation](https://modelcontextprotocol.io/)
- [Umbraco MCP Package](https://www.npmjs.com/package/@umbraco-cms/mcp-dev)
- [Playwright MCP](https://www.npmjs.com/package/@playwright/mcp)
- [Claude Code Documentation](https://docs.anthropic.com/claude-code)
