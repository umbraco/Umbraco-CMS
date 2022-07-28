import '@umbraco-ui/uui-css/dist/uui-css.css';
import '@umbraco-ui/uui';

export const parameters = {
	actions: { argTypesRegex: '^on[A-Z].*' },
	controls: {
		matchers: {
			color: /(background|color)$/i,
			date: /Date$/,
		},
	},
};
