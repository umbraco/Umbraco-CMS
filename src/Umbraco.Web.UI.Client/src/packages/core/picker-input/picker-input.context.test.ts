import { UmbPickerInputContext } from './picker-input.context.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_ROUTE_CONTEXT, UMB_ROUTE_PATH_ADDENDUM_CONTEXT } from '@umbraco-cms/backoffice/router';
import type { UmbModalRouteRegistration } from '@umbraco-cms/backoffice/router';

// A route context stand-in that records the path each modal registration resolves to, without
// requiring a real router-slot (and its base-path resolution) to be mounted.
class UmbTestRouteContext {
	readonly registeredPaths: Array<string> = [];

	constructor(private host: HTMLElement) {}

	getHostElement() {
		return this.host;
	}

	registerModal(registration: UmbModalRouteRegistration) {
		this.registeredPaths.push(registration.generateModalPath());
	}

	unregisterModal() {
		// noop
	}
}

// A fixed, root-level route path addendum, standing in for umb-router-slot's own reset context.
class UmbTestRoutePathAddendumContext {
	readonly addendum = new UmbStringState('').asObservable();

	constructor(private host: HTMLElement) {}

	getHostElement() {
		return this.host;
	}
}

@customElement('test-picker-input-context-host')
class UmbTestPickerInputContextHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbPickerInputContext', () => {
	let routeContext: UmbTestRouteContext;
	// Two sibling elements, both hosting a picker input context registered for the same modal
	// alias, outside of umb-property (which would otherwise namespace them by property alias).
	// This is the shape of two entity-data-backed pickers rendered via umb-property-layout in the
	// same view.
	let hostOne: UmbTestPickerInputContextHostElement;
	let hostTwo: UmbTestPickerInputContextHostElement;
	let providers: Array<{ destroy(): void }>;

	beforeEach(() => {
		hostOne = new UmbTestPickerInputContextHostElement();
		hostTwo = new UmbTestPickerInputContextHostElement();
		document.body.appendChild(hostOne);
		document.body.appendChild(hostTwo);

		routeContext = new UmbTestRouteContext(hostOne);
		providers = [];

		for (const host of [hostOne, hostTwo]) {
			const routeProvider = new UmbContextProvider(host, UMB_ROUTE_CONTEXT, routeContext as never);
			const addendumProvider = new UmbContextProvider(
				host,
				UMB_ROUTE_PATH_ADDENDUM_CONTEXT,
				new UmbTestRoutePathAddendumContext(host) as never,
			);
			routeProvider.hostConnected();
			addendumProvider.hostConnected();
			providers.push(routeProvider, addendumProvider);
		}
	});

	afterEach(() => {
		providers.forEach((provider) => provider.destroy());
		providers = [];
		document.body.innerHTML = '';
	});

	async function waitForRegistrations(count: number): Promise<Array<string>> {
		for (let attempt = 0; attempt < 100; attempt++) {
			if (routeContext.registeredPaths.length >= count) return routeContext.registeredPaths;
			await new Promise((resolve) => setTimeout(resolve, 10));
		}
		throw new Error(`Timed out waiting for ${count} modal route registration(s).`);
	}

	it('registers distinct modal routes for two pickers sharing a modal alias when each sets a unique path segment', async () => {
		// Register first with no segment, matching real usage: umb-input-entity-data subscribes to
		// modalRoute in its constructor, before Lit applies the picker-alias property on first render.
		const first = new UmbPickerInputContext(hostOne, 'Umb.Repository.PickerInputTest', 'Umb.Modal.PickerInputTest');
		first.observe(first.modalRoute, () => undefined);

		const second = new UmbPickerInputContext(hostTwo, 'Umb.Repository.PickerInputTest', 'Umb.Modal.PickerInputTest');
		second.observe(second.modalRoute, () => undefined);

		const [collidedFirstPath, collidedSecondPath] = await waitForRegistrations(2);
		expect(collidedFirstPath).to.equal(collidedSecondPath);

		// Setting the segments now must destroy and re-register each picker's route.
		first.setUniquePathSegment('document-blueprint');
		second.setUniquePathSegment('element');

		const registeredPaths = await waitForRegistrations(4);
		const [firstPath, secondPath] = registeredPaths.slice(2);

		expect(firstPath).to.not.equal(secondPath);
		expect(firstPath).to.not.equal(collidedFirstPath);
		expect(secondPath).to.not.equal(collidedSecondPath);

		first.destroy();
		second.destroy();
	});

	it('keeps its existing modal route when no unique path segment is set', async () => {
		const context = new UmbPickerInputContext(hostOne, 'Umb.Repository.PickerInputTest', 'Umb.Modal.PickerInputTest');
		context.observe(context.modalRoute, () => undefined);

		const [path] = await waitForRegistrations(1);

		expect(path).to.equal('modal/umb-modal-pickerinputtest');

		context.destroy();
	});
});
