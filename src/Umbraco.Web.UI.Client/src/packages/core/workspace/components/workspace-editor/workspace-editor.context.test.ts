import type { ManifestWorkspaceView } from '../../types.js';
import { UmbWorkspaceEditorContext } from './workspace-editor.context.js';
import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { filter, firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('test-workspace-editor-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Stands in for the workspace view element that a view context is provided at.
@customElement('test-workspace-editor-view')
class UmbTestWorkspaceViewElement extends UmbElementMixin(HTMLElement) {}

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

/**
 * Resolves with the first value of the observable that satisfies `predicate`, or
 * `undefined` if none arrives within `timeoutMs`.
 */
function waitForValue<T>(
	observable: Observable<T>,
	predicate: (value: T) => boolean,
	timeoutMs: number,
): Promise<T | undefined> {
	return Promise.race([
		firstValueFrom(observable.pipe(filter(predicate))),
		new Promise<undefined>((resolve) => setTimeout(() => resolve(undefined), timeoutMs)),
	]);
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

	// A view leaves the permitted list when its manifest is unregistered or one of its
	// conditions stops being permitted. The bug was that only an added view counted as a
	// change: the context of a removed view was destroyed but stayed in `views`, and was
	// handed out again when the view returned, so providing it threw.
	describe('when a view is no longer permitted', () => {
		const view2Manifest = manifests.find((m) => m.alias === VIEW_2_ALIAS)!;

		afterEach(() => {
			if (!umbExtensionsRegistry.isRegistered(VIEW_2_ALIAS)) {
				umbExtensionsRegistry.register(view2Manifest);
			}
		});

		it('re-emits views without the removed view', async () => {
			await firstValueFrom(context.views.pipe(filter((views) => views.length === manifests.length)));

			umbExtensionsRegistry.unregister(VIEW_2_ALIAS);

			const views = await waitForValue(context.views, (views) => views.length === manifests.length - 1, 500);

			expect(views, 'views did not re-emit after a view was removed').to.not.be.undefined;
			expect(views!.map((view) => view.manifest.alias)).to.deep.equal([VIEW_1_ALIAS]);
		});

		it('creates a new view context when the view is permitted again', async () => {
			const initialViews = await firstValueFrom(
				context.views.pipe(filter((views) => views.length === manifests.length)),
			);
			const removedView = initialViews.find((view) => view.manifest.alias === VIEW_2_ALIAS);

			umbExtensionsRegistry.unregister(VIEW_2_ALIAS);
			await waitForValue(context.views, (views) => views.length === manifests.length - 1, 500);

			umbExtensionsRegistry.register(view2Manifest);
			const views = await waitForValue(
				context.views,
				(views) => views.length === manifests.length && !views.includes(removedView!),
				500,
			);

			expect(views, 'views did not re-emit with a new context for the returning view').to.not.be.undefined;
			const returnedView = views!.find((view) => view.manifest.alias === VIEW_2_ALIAS);
			expect(returnedView).to.not.equal(removedView);
			// A destroyed view context can no longer be provided, which is what breaks the route setup.
			const viewElement = new UmbTestWorkspaceViewElement();
			expect(() => returnedView!.provideAt(viewElement)).to.not.throw();
			viewElement.destroy();
		});
	});
});
