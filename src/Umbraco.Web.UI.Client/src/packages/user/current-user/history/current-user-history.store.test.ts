import { UmbCurrentUserHistoryStore } from './current-user-history.store.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
// Reach into the module directly for the internal setter; the public barrel
// intentionally only exposes the observable, not the writer.
import { _setUmbCurrentViewTitle } from '../../../core/view/context/current-view-title.js';
import type { UmbCurrentViewTitleSegment } from '@umbraco-cms/backoffice/view';
import { UmbId } from '@umbraco-cms/backoffice/id';

@customElement('test-current-user-history-store-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type NavigateListener = (event: { destination: { url: string } }) => void;

function installNavigationMock(origin = window.location.origin) {
	const listeners: Record<string, NavigateListener[]> = {};
	const previous = (window as any).navigation;
	(window as any).navigation = {
		addEventListener: (type: string, cb: NavigateListener) => {
			(listeners[type] ||= []).push(cb);
		},
		removeEventListener: (type: string, cb: NavigateListener) => {
			listeners[type] = (listeners[type] ?? []).filter((f) => f !== cb);
		},
	};
	return {
		fire(path: string) {
			(listeners.navigate ?? []).forEach((cb) => cb({ destination: { url: origin + path } }));
		},
		restore() {
			if (previous === undefined) {
				delete (window as any).navigation;
			} else {
				(window as any).navigation = previous;
			}
		},
	};
}

function segment(label: string, kind: UmbCurrentViewTitleSegment['kind']): UmbCurrentViewTitleSegment {
	return { label, kind };
}

describe('UmbCurrentUserHistoryStore', () => {
	let host: UmbTestControllerHostElement;
	let store: UmbCurrentUserHistoryStore;
	let nav: ReturnType<typeof installNavigationMock>;

	beforeEach(() => {
		nav = installNavigationMock();
		host = new UmbTestControllerHostElement();
		store = new UmbCurrentUserHistoryStore(host);
	});

	afterEach(() => {
		store.destroy();
		nav.restore();
		_setUmbCurrentViewTitle(undefined);
	});

	function currentItems() {
		let items: ReadonlyArray<any> = [];
		store.history.subscribe((v) => (items = v as any)).unsubscribe();
		return items;
	}

	function publish(path: string, ...segments: UmbCurrentViewTitleSegment[]) {
		_setUmbCurrentViewTitle({ path, segments });
	}

	it('creates a history entry with path-derived label and no displayPath on navigate', () => {
		nav.fire('/umbraco/section/users/workspace/user/edit/abc');
		const items = currentItems();
		expect(items).to.have.length(1);
		expect(items[0].path).to.equal('/umbraco/section/users/workspace/user/edit/abc');
		expect(items[0].displayPath).to.equal(undefined);
	});

	it('uses the leaf non-tab segment as label and earlier segments as breadcrumb', () => {
		const path = '/umbraco/section/users/workspace/user/edit/abc';
		nav.fire(path);
		publish(path, segment('Users', 'section'), segment('User', 'workspace-type'), segment('Amelie Walker', 'workspace'));
		const item = currentItems()[0];
		expect(item.label).to.equal('Amelie Walker');
		expect(item.displayPath).to.equal('Users › User');
	});

	it('filters tab segments from both the label and the breadcrumb', () => {
		// Full URL (including tab) is preserved on `path` for click-through, but the tab name isn't surfaced.
		const path = '/umbraco/section/users/workspace/user/edit/abc';
		nav.fire(path);
		publish(
			path,
			segment('Users', 'section'),
			segment('User', 'workspace-type'),
			segment('Amelie Walker', 'workspace'),
			segment('Details', 'tab'),
		);
		const item = currentItems()[0];
		expect(item.label).to.equal('Amelie Walker');
		expect(item.displayPath).to.equal('Users › User');
	});

	it('includes workspace-ancestor segments in the breadcrumb', () => {
		const path = '/umbraco/section/media/workspace/media/edit/abc';
		nav.fire(path);
		publish(
			path,
			segment('Media', 'section'),
			segment('People', 'workspace-ancestor'),
			segment('John Doe', 'workspace'),
			segment('Details', 'tab'),
		);
		const item = currentItems()[0];
		expect(item.label).to.equal('John Doe');
		expect(item.displayPath).to.equal('Media › People');
	});

	it('distinguishes a user group from a user with the same breadcrumb slot', () => {
		const path = '/umbraco/section/users/workspace/user-group/edit/admins';
		nav.fire(path);
		publish(
			path,
			segment('Users', 'section'),
			segment('User Group', 'workspace-type'),
			segment('Administrators', 'workspace'),
			segment('Details', 'tab'),
		);
		const item = currentItems()[0];
		expect(item.label).to.equal('Administrators');
		expect(item.displayPath).to.equal('Users › User Group');
	});

	it('leaves displayPath undefined when there is only one non-tab segment', () => {
		const path = '/umbraco/section/users';
		nav.fire(path);
		publish(path, segment('Users', 'section'));
		const item = currentItems()[0];
		expect(item.label).to.equal('Users');
		expect(item.displayPath).to.equal(undefined);
	});

	it('ignores updates with only tab segments (no breadcrumb content)', () => {
		const path = '/umbraco/section/users/workspace/user/edit/abc';
		nav.fire(path);
		const labelBefore = currentItems()[0].label;
		publish(path, segment('Details', 'tab'));
		expect(currentItems()[0].label).to.equal(labelBefore);
	});

	it('ignores modal emissions that would otherwise overwrite the current page entry', () => {
		// Regression: opening a modal (e.g. the current-user profile dialog) must not
		// rewrite the underlying page's history entry, because modals don't change the URL.
		const path = '/umbraco/section/member-management/workspace/member-root';
		nav.fire(path);
		publish(path, segment('Members', 'section'), segment('Members', 'workspace'));
		const labelBeforeModal = currentItems()[0].label;

		publish(path, segment('User', 'modal'));

		expect(currentItems()[0].label).to.equal(labelBeforeModal);
		expect(currentItems()[0].label).to.equal('Members');
	});

	it('does not split entity names containing " | "', () => {
		const path = '/umbraco/section/content/workspace/document/edit/abc';
		nav.fire(path);
		publish(path, segment('Content', 'section'), segment('A | B', 'workspace'));
		const item = currentItems()[0];
		expect(item.label).to.equal('A | B');
		expect(item.displayPath).to.equal('Content');
	});

	it('ignores stale view title emissions during fast navigation', () => {
		nav.fire('/umbraco/section/users');
		nav.fire('/umbraco/section/content');
		// Late arrival from the previous page must not land on the new entry.
		publish('/umbraco/section/users', segment('Users', 'section'), segment('John Doe', 'workspace'));
		const items = currentItems();
		const latest = items[items.length - 1];
		expect(latest.path).to.equal('/umbraco/section/content');
		expect(latest.label).to.equal('Content');
	});

	it('removes an entry ending in a GUID if no title ever arrived for it', () => {
		const guid = UmbId.new();
		const path = `/umbraco/section/content/workspace/document/edit/${guid}`;
		nav.fire(path);
		nav.fire('/umbraco/section/users');
		expect(currentItems().every((i) => i.path !== path)).to.equal(true);
	});

	it('keeps an entry ending in a GUID once the title is resolved', () => {
		const guid = UmbId.new();
		const path = `/umbraco/section/content/workspace/document/edit/${guid}`;
		nav.fire(path);
		publish(path, segment('Content', 'section'), segment('My Page', 'workspace'));
		nav.fire('/umbraco/section/users');
		expect(currentItems().some((i) => i.path === path && i.label === 'My Page')).to.equal(true);
	});

	it('clears lastAdded tracking on clear()', () => {
		nav.fire('/umbraco/section/users');
		expect(currentItems()).to.have.length(1);
		store.clear();
		expect(currentItems()).to.have.length(0);
		// A subsequent title emission must not resurrect any entry.
		publish('/umbraco/section/users', segment('Users', 'section'));
		expect(currentItems()).to.have.length(0);
	});
});
