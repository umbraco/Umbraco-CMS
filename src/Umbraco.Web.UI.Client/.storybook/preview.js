import '@umbraco-ui/uui-css/dist/uui-css.css';
import '@umbraco-ui/uui';
// This imports and runs the MSW
import '../src/index.ts';

export const parameters = {
	actions: { argTypesRegex: '^on[A-Z].*' },
	controls: {
		matchers: {
			color: /(background|color)$/i,
			date: /Date$/,
		},
	},
};
