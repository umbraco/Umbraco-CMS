import '../src/css/custom-properties.css';
import '@umbraco-ui/uui';

import { initialize, mswDecorator } from 'msw-storybook-addon';

import { onUnhandledRequest } from '../src/mocks/browser';
import { handlers } from '../src/mocks/handlers';

// Initialize MSW
initialize({onUnhandledRequest});

// Provide the MSW addon decorator globally
export const decorators = [mswDecorator];

export const parameters = {
	actions: { argTypesRegex: '^on.*' },
	controls: {
		expanded: true,
		matchers: {
			color: /(background|color)$/i,
			date: /Date$/,
		},
	},
	msw: {
		handlers: {
			global: handlers
		}
	}
};
