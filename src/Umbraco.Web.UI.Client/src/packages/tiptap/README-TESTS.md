# Tiptap Unit Tests

This directory contains comprehensive unit tests for the Tiptap rich text editor components in Umbraco CMS.

## Test Coverage

### Core Components
- **`input-tiptap.test.ts`** - Core input component functionality, value handling, form control behavior
- **`input-tiptap-validation.test.ts`** - Validation scenarios, edge cases, state management, lifecycle
- **`tiptap-toolbar-statusbar.test.ts`** - Toolbar and statusbar components testing

### Property Editor
- **`property-editor-ui-tiptap.test.ts`** - Property editor UI tests with change events and markup processing

### Extensions
- **`base.test.ts`** - Extension base classes testing with toolbar element behavior
- **`core-extensions.test.ts`** - Core extension APIs (underline, link, word-count)
- **`rich-text-essentials.test.ts`** - Rich text essentials extension testing
- **`toolbar-extensions.test.ts`** - Toolbar extensions (bold, italic, underline) with command execution
- **`statusbar-extensions.test.ts`** - Statusbar extensions (word count, element path)

### Context Management
- **`tiptap-rte.context.test.ts`** - Context management testing

### Integration
- **`integration.test.ts`** - Integration tests for components working together

## Test Features

### Component Testing
- Component initialization and lifecycle
- Property setting/getting and validation
- Event handling and state management
- Error handling and edge cases
- Accessibility compliance (when available)

### Extension Testing
- Extension loading and configuration
- Tiptap API integration
- Command execution and state management
- Configuration-based behavior

### Integration Testing
- Property editor to input component communication
- Configuration flow from UI to components
- Value processing and block element handling
- Context management across components

## Running Tests

Tests are designed to run with web-test-runner using Playwright:

```bash
# Run all Tiptap tests
npm test -- --files "**/tiptap/**/*.test.ts"

# Run tests in development mode
npm run test:dev -- --files "**/tiptap/**/*.test.ts"

# Run specific test file
npm test -- --files "**/input-tiptap.test.ts"
```

## Test Structure

Each test file follows the established patterns:
- Uses `@open-wc/testing` for component fixtures
- Includes accessibility testing when available
- Proper setup and cleanup in beforeEach/afterEach
- Comprehensive error and edge case handling
- Mock implementations for external dependencies

## Dependencies

Tests depend on:
- `@open-wc/testing` for component testing
- `@umbraco-cms/internal/test-utils` for test utilities
- `@web/test-runner-playwright` for browser automation
- Various Umbraco packages for type definitions and utilities