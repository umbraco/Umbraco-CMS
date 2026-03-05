# SQLite to Mock

Transforms an Umbraco SQLite database into a TypeScript mock data set for use with the backoffice Mock Service Worker (MSW) setup.

## What it does

Reads a real Umbraco SQLite database and generates typed mock data files for:

- Data types
- Document types & media types
- Documents & media
- Users & user groups
- Templates
- Languages
- Dictionary items

It also generates supporting files (index, placeholders, static data) so the output is a complete mock data set ready to use.

## Setup

```bash
npm install
```

## Usage

```bash
npm run generate -- <db-path> <set-alias>
```

- **db-path** - Path to an Umbraco SQLite database file (absolute or relative)
- **set-alias** - Name for the generated mock data set folder

### Example

```bash
npm run generate -- /path/to/Umbraco.sqlite my-site
```

This outputs files to `mocks/data/sets/my-site/` and runs eslint to format them.

## Testing the generated set

Start the backoffice with:

```
VITE_MOCK_SET=my-site npm run dev
```

You must also register the new set in `mocks/mock-manager.ts` by adding an entry to the `#mockSetLoaders` map:

```ts
#mockSetLoaders: Record<string, () => Promise<UmbMockDataSet>> = {
  default: () => import('./data/sets/default/index.js') as Promise<UmbMockDataSet>,
  'my-site': () => import('./data/sets/my-site/index.js') as Promise<UmbMockDataSet>,
};
```
