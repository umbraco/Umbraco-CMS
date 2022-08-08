import '@umbraco-ui/uui';
import '@umbraco-ui/uui-css/dist/uui-css.css';

import { initialize, mswDecorator } from 'msw-storybook-addon';

import { onUnhandledRequest } from '../src/mocks/browser';
import { handlers } from '../src/mocks/browser-handlers';

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
