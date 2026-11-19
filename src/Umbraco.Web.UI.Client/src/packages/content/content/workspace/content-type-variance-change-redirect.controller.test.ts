import { expect } from '@open-wc/testing';
import { UmbContentTypeVarianceChangeRedirectController } from './content-type-variance-change-redirect.controller.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbStaleVariantRouteResolverVariant } from '@umbraco-cms/backoffice/workspace';

const WORKSPACE_ROUTE = '/section/content/workspace/document/edit/123';
const MODAL_PATH = '/view/info/modal/umb-modal-workspace/general/document-type/edit/456';

const EN = { culture: 'en-US', segment: null, unique: 'en-US' };
const DA = { culture: 'da', segment: null, unique: 'da' };
const EN_SEG = { culture: 'en-US', segment: 'seg1', unique: 'en-US_seg1' };
const INVARIANT = { culture: null, segment: null, unique: 'invariant' };

@customElement('umb-test-variance-redirect-workspace-host')
class UmbTestWorkspaceHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	variesByCulture?: boolean;
	variesBySegment?: boolean;
	variantOptions: Array<UmbStaleVariantRouteResolverVariant> = [];

	splitView = {
		getWorkspaceRoute: () => WORKSPACE_ROUTE,
	};

	getVariesByCulture() {
		return this.variesByCulture;
	}

	getVariesBySegment() {
		return this.variesBySegment;
	}

	async getVariantOptions() {
		return this.variantOptions;
	}
}

describe('UmbContentTypeVarianceChangeRedirectController', () => {
	let host: UmbTestWorkspaceHostElement;
	let controller: UmbContentTypeVarianceChangeRedirectController;
	let originalUrl: string;

	beforeEach(() => {
		originalUrl = window.location.pathname + window.location.search;
		host = new UmbTestWorkspaceHostElement();
		controller = new UmbContentTypeVarianceChangeRedirectController(host);
	});

	afterEach(() => {
		controller.destroy();
		history.replaceState(null, '', originalUrl);
	});

	function setType(variesByCulture: boolean, variesBySegment: boolean, variantOptions: typeof host.variantOptions) {
		host.variesByCulture = variesByCulture;
		host.variesBySegment = variesBySegment;
		host.variantOptions = variantOptions;
	}

	/** Navigates like the router does, which dispatches changestate on every history change. */
	async function navigate(path: string) {
		history.replaceState(null, '', path);
		window.dispatchEvent(new Event('changestate'));
		await new Promise((resolve) => setTimeout(resolve));
	}

	it('does not redirect when the modal closes without a variance change, even if the URL refers to a missing variant', async () => {
		setType(true, false, [EN, DA]);
		await navigate(`${WORKSPACE_ROUTE}/fr${MODAL_PATH}`);

		controller.trackVarianceChange();
		await navigate(`${WORKSPACE_ROUTE}/fr`);

		expect(window.location.pathname).to.equal(`${WORKSPACE_ROUTE}/fr`);
	});

	it('does not redirect when the variance change was not tracked', async () => {
		setType(true, false, [EN, DA]);
		setType(false, false, [INVARIANT]);
		await navigate(`${WORKSPACE_ROUTE}/en-US`);

		expect(window.location.pathname).to.equal(`${WORKSPACE_ROUTE}/en-US`);
	});

	it('redirects to an existing variant after a culture variance change, keeping the trailing path and query', async () => {
		setType(true, false, [EN, DA]);
		await navigate(`${WORKSPACE_ROUTE}/en-US${MODAL_PATH}`);

		controller.trackVarianceChange();
		setType(false, false, [INVARIANT]);
		await navigate(`${WORKSPACE_ROUTE}/en-US/view/info?foo=bar`);

		expect(window.location.pathname).to.equal(`${WORKSPACE_ROUTE}/invariant/view/info`);
		expect(window.location.search).to.equal('?foo=bar');
	});

	it('redirects to an existing variant after a segment variance change', async () => {
		setType(true, true, [EN, EN_SEG]);
		await navigate(`${WORKSPACE_ROUTE}/en-US_seg1${MODAL_PATH}`);

		controller.trackVarianceChange();
		setType(true, false, [EN, DA]);
		await navigate(`${WORKSPACE_ROUTE}/en-US_seg1`);

		expect(window.location.pathname).to.equal(`${WORKSPACE_ROUTE}/en-US`);
	});

	it('waits for the modal to close before redirecting', async () => {
		setType(true, false, [EN, DA]);
		await navigate(`${WORKSPACE_ROUTE}/en-US${MODAL_PATH}`);

		controller.trackVarianceChange();
		setType(false, false, [INVARIANT]);
		await navigate(`${WORKSPACE_ROUTE}/en-US${MODAL_PATH}/view/settings`);
		expect(window.location.pathname).to.equal(`${WORKSPACE_ROUTE}/en-US${MODAL_PATH}/view/settings`);

		await navigate(`${WORKSPACE_ROUTE}/en-US/view/info`);
		expect(window.location.pathname).to.equal(`${WORKSPACE_ROUTE}/invariant/view/info`);
	});

	it('stops tracking once the modal has closed', async () => {
		setType(true, false, [EN, DA]);
		await navigate(`${WORKSPACE_ROUTE}/en-US${MODAL_PATH}`);

		controller.trackVarianceChange();
		await navigate(`${WORKSPACE_ROUTE}/en-US`);

		setType(false, false, [INVARIANT]);
		await navigate(`${WORKSPACE_ROUTE}/en-US`);

		expect(window.location.pathname).to.equal(`${WORKSPACE_ROUTE}/en-US`);
	});
});
