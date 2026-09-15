import { UmbWorkspaceSplitViewContext } from './workspace-split-view.context.js';
import { UMB_VARIANT_WORKSPACE_CONTEXT } from '../../contexts/index.js';
import { UmbWorkspaceSplitViewManager } from '../../controllers/workspace-split-view-manager.controller.js';
import { expect, fixture } from '@open-wc/testing';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

@customElement('umb-test-workspace-split-view-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestWorkspaceSplitViewHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type UmbTestVariantOption = { unique: string; culture: string | null; segment: string | null; name: string };

/**
 * A minimal stand-in for `UmbVariantDatasetWorkspaceContext` — only the members `UmbWorkspaceSplitViewContext`
 * actually reads. `UMB_VARIANT_WORKSPACE_CONTEXT`'s type guard narrows on `'variants' in context`, so a `variants`
 * property must exist even though nothing under test reads it.
 */
class UmbTestVariantWorkspaceContext extends UmbContextBase {
	public variants = new UmbBasicState<Array<unknown>>([]).asObservable();
	public isNew = new UmbBasicState(false).asObservable();
	public splitView: UmbWorkspaceSplitViewManager;
	public variantOptionsState: UmbArrayState<UmbTestVariantOption>;
	public createCallCount = 0;
	public onCreate?: () => void;

	constructor(
		host: UmbControllerHostElement,
		splitView: UmbWorkspaceSplitViewManager,
		variantOptionsState: UmbArrayState<UmbTestVariantOption>,
	) {
		super(host, UMB_VARIANT_WORKSPACE_CONTEXT);
		this.splitView = splitView;
		this.variantOptionsState = variantOptionsState;
	}

	public get variantOptions() {
		return this.variantOptionsState.asObservable();
	}

	public getVariantValidationContext() {
		return undefined;
	}

	public createPropertyDatasetContext() {
		this.createCallCount++;
		this.onCreate?.();
		return { destroy: () => {} };
	}
}

describe('UmbWorkspaceSplitViewContext', () => {
	let host: UmbControllerHostElement;
	let splitView: UmbWorkspaceSplitViewManager;
	let variantOptions: UmbArrayState<UmbTestVariantOption>;
	let workspaceContext: UmbTestVariantWorkspaceContext;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-workspace-split-view-host></umb-test-workspace-split-view-host>`);

		splitView = new UmbWorkspaceSplitViewManager();
		splitView.setActiveVariant(0, 'en-us', null);

		variantOptions = new UmbArrayState<UmbTestVariantOption>(
			[{ unique: 'en-us', culture: 'en-us', segment: null, name: 'Original name' }],
			(x) => x.unique,
		);

		workspaceContext = new UmbTestVariantWorkspaceContext(host, splitView, variantOptions);
	});

	afterEach(() => {
		workspaceContext.destroy();
	});

	async function waitForFirstDatasetContext(): Promise<void> {
		if (workspaceContext.createCallCount > 0) return;
		await new Promise<void>((resolve) => {
			workspaceContext.onCreate = resolve;
		});
		workspaceContext.onCreate = undefined;
	}

	it('creates the dataset context once the active variant resolves', async () => {
		const ctx = new UmbWorkspaceSplitViewContext(host);
		ctx.setSplitViewIndex(0);

		await waitForFirstDatasetContext();

		expect(workspaceContext.createCallCount).to.equal(1);
	});

	it('does not recreate the dataset context when an unrelated variant field changes', async () => {
		const ctx = new UmbWorkspaceSplitViewContext(host);
		ctx.setSplitViewIndex(0);
		await waitForFirstDatasetContext();
		expect(workspaceContext.createCallCount).to.equal(1);

		// `variantOptions` is rebuilt (with brand-new option objects) whenever any variant field changes — e.g. the
		// content name — even though the variant being edited (en-us) hasn't actually changed. This must not tear
		// down and rebuild the dataset context, or every such unrelated change would reset the property tree's
		// validation state underneath it.
		variantOptions.setValue([{ unique: 'en-us', culture: 'en-us', segment: null, name: 'Changed name' }]);
		await new Promise((resolve) => setTimeout(resolve, 0));

		expect(workspaceContext.createCallCount).to.equal(1);
	});

	it('recreates the dataset context when the variant disappears and reappears', async () => {
		const ctx = new UmbWorkspaceSplitViewContext(host);
		ctx.setSplitViewIndex(0);
		await waitForFirstDatasetContext();
		expect(workspaceContext.createCallCount).to.equal(1);

		// The variant is removed (e.g. the culture was unpublished/deleted) ...
		variantOptions.setValue([]);
		await new Promise((resolve) => setTimeout(resolve, 0));
		expect(workspaceContext.createCallCount).to.equal(1);

		// ... and reappears — this is a genuine identity change and must recreate the dataset context.
		const secondCreate = new Promise<void>((resolve) => {
			workspaceContext.onCreate = resolve;
		});
		variantOptions.setValue([{ unique: 'en-us', culture: 'en-us', segment: null, name: 'Original name' }]);
		await secondCreate;

		expect(workspaceContext.createCallCount).to.equal(2);
	});
});
