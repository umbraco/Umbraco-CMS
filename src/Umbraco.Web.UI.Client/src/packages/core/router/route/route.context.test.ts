import { UmbRouteContext } from './route.context.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbModalRouteRegistration } from '../modal-registration/modal-route-registration.interface.js';
import type { IRouterSlot } from '../router-slot/index.js';

// A modal route registration stand-in that reports a fixed, pre-computed path, so registerModal's
// duplicate-path check can be exercised without a real UmbModalRouteRegistrationController.
class UmbTestModalRouteRegistration {
	constructor(private path: string) {}

	generateModalPath() {
		return this.path;
	}
}

function createTestModalRouter(): IRouterSlot {
	const element = document.createElement('div');
	Object.assign(element, { routes: [], render: () => Promise.resolve() });
	return element as unknown as IRouterSlot;
}

@customElement('test-route-context-host')
class UmbTestRouteContextHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbRouteContext.registerModal', () => {
	let hostElement: UmbTestRouteContextHostElement;
	let routeContext: UmbRouteContext;
	let originalWarn: typeof console.warn;
	let warnings: Array<string>;

	beforeEach(() => {
		hostElement = new UmbTestRouteContextHostElement();
		document.body.appendChild(hostElement);
		routeContext = new UmbRouteContext(hostElement, createTestModalRouter(), createTestModalRouter());

		warnings = [];
		originalWarn = console.warn;
		console.warn = (...args: Array<unknown>) => {
			warnings.push(args.map(String).join(' '));
		};
	});

	afterEach(() => {
		console.warn = originalWarn;
		routeContext.destroy();
		document.body.innerHTML = '';
	});

	it('warns once, naming the path, when a second registration resolves to an already-registered path', () => {
		const first = new UmbTestModalRouteRegistration('modal/umb-modal-treepicker/element') as unknown as UmbModalRouteRegistration;
		const second = new UmbTestModalRouteRegistration('modal/umb-modal-treepicker/element') as unknown as UmbModalRouteRegistration;

		routeContext.registerModal(first);
		expect(warnings).to.have.lengthOf(0);

		routeContext.registerModal(second);

		expect(warnings).to.have.lengthOf(1);
		expect(warnings[0]).to.contain('modal/umb-modal-treepicker/element');
	});

	it('does not warn when two registrations resolve to distinct paths', () => {
		const first = new UmbTestModalRouteRegistration('modal/umb-modal-treepicker/element') as unknown as UmbModalRouteRegistration;
		const second = new UmbTestModalRouteRegistration(
			'modal/umb-modal-treepicker/document-blueprint',
		) as unknown as UmbModalRouteRegistration;

		routeContext.registerModal(first);
		routeContext.registerModal(second);

		expect(warnings).to.have.lengthOf(0);
	});
});
