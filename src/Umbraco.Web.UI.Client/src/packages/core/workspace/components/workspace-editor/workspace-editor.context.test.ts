import type { ManifestWorkspaceView } from '../../types.js';
import { UmbWorkspaceEditorContext } from './workspace-editor.context.js';
import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { filter, firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('test-workspace-editor-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const VIEW_1_ALIAS = 'UmbTest.WorkspaceEditor.View.1';
const VIEW_2_ALIAS = 'UmbTest.WorkspaceEditor.View.2';

const OVERRIDDEN_LABEL = 'Overridden label';
const OVERRIDDEN_ICON = 'icon-checkbox';

const manifests: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: VIEW_1_ALIAS,
		name: 'Test Workspace View 1',
		weight: 200,
		meta: { label: 'View 1', icon: 'icon-document', pathname: 'view-1' },
	},
	{
		type: 'workspaceView',
		alias: VIEW_2_ALIAS,
		name: 'Test Workspace View 2',
		weight: 100,
		meta: { label: 'View 2', icon: 'icon-document', pathname: 'view-2' },
	},
];

/**
 * Resolves once the observable emits *after* the initial (replayed) value that
 * subscribing synchronously delivers, or `false` if nothing new arrives within
 * `timeoutMs`.
 */
function waitForNextEmission(observable: Observable<unknown>, timeoutMs: number): Promise<boolean> {
	return new Promise((resolve) => {
		let sawBaseline = false;
		let settled = false;
		const settle = (result: boolean) => {
			if (settled) return;
			settled = true;
			clearTimeout(timer);
			subscription.unsubscribe();
			resolve(result);
		};
		const subscription = observable.subscribe(() => {
			if (!sawBaseline) {
				sawBaseline = true;
				return;
			}
			settle(true);
		});
		const timer = setTimeout(() => settle(false), timeoutMs);
	});
}

describe('UmbWorkspaceEditorContext', () => {
	let hostElement: UmbTestControllerHostElement;
	let context: UmbWorkspaceEditorContext;

	before(() => {
		umbExtensionsRegistry.registerMany(manifests);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(manifests.map((m) => m.alias));
	});

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		context = new UmbWorkspaceEditorContext(hostElement);
	});

	afterEach(() => {
		hostElement.destroy();
	});

	// Regression for #23311: a late-arriving override that changes a view's manifest
	// (icon/label) must re-emit `views`. The bug was that the merged value kept the
	// same array reference, so `distinctUntilChanged` suppressed the update and the
	// workspace view header never refreshed.
	it('re-emits views when an override changes a view manifest', async () => {
		// Wait for the async extension initializer to surface the registered views.
		await firstValueFrom(context.views.pipe(filter((views) => views.length === manifests.length)));

		// Arm the waiter before mutating overrides so the baseline replay is consumed first.
		const emittedAgain = waitForNextEmission(context.views, 250);

		context.setOverrides([{ alias: VIEW_1_ALIAS, meta: { label: OVERRIDDEN_LABEL, icon: OVERRIDDEN_ICON } }]);

		expect(await emittedAgain, 'views did not re-emit after the override changed').to.be.true;

		const views = await firstValueFrom(context.views);
		const overriddenView = views.find((view) => view.manifest.alias === VIEW_1_ALIAS);
		expect(overriddenView?.manifest.meta.label).to.equal(OVERRIDDEN_LABEL);
		expect(overriddenView?.manifest.meta.icon).to.equal(OVERRIDDEN_ICON);
	});
});
