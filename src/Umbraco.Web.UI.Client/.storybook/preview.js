import '@umbraco-ui/uui';
import '@umbraco-ui/uui-css/dist/uui-css.css';

export const parameters = {
	actions: { argTypesRegex: '^on.*' },
	controls: {
		expanded: true,
		matchers: {
			color: /(background|color)$/i,
			date: /Date$/,
		},
	},
};
