import { umbExtension } from '../../src/libs/extension-api/decorators/umb-extension.decorator.js';

class MyDecoratorDashboardElement extends HTMLElement {
	connectedCallback() {
		this.innerHTML = '<h1>Decorator Dashboard</h1><p>Registered via umbExtension() — vanilla JS, no build step.</p>';
	}
}

customElements.define('my-decorator-dashboard', MyDecoratorDashboardElement);

// Called imperatively because browsers don't support the @decorator syntax in raw JS yet.
// With a build step (TypeScript/Vite), you'd write: @umbExtension({ ... }) class MyElement { }
umbExtension({
	type: 'dashboard',
	alias: 'My.Dashboard.Decorator',
	name: 'Decorator Dashboard',
	weight: 50,
	meta: {
		label: 'Decorator (Vanilla JS)',
		pathname: 'decorator-vanilla',
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionAlias',
			match: 'Umb.Section.Content',
		},
	],
})(MyDecoratorDashboardElement);

export default MyDecoratorDashboardElement;
