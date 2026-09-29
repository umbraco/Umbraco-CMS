import { aTimeout, expect } from '@open-wc/testing';
import { firstValueFrom, Observable, of } from '@umbraco-cms/backoffice/external/rxjs';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbTreeItemActiveManager } from './tree-active-manager';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbTreeItemActiveManager', () => {
	let manager: UmbTreeItemActiveManager;
	const item = { entityType: 'test', unique: '123' };
	const item2 = { entityType: 'test', unique: '456' };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbTreeItemActiveManager(hostElement);
	});

	describe('isCurrentLocation', () => {
		const currentPath = () => window.location.pathname;

		it('is true while the browser location points at the path', async () => {
			const isActive = await firstValueFrom(manager.isCurrentLocation(of(currentPath())));
			expect(isActive).to.be.true;
		});

		it('is false for a path the browser is not on', async () => {
			const isActive = await firstValueFrom(manager.isCurrentLocation(of('/some/other/path')));
			expect(isActive).to.be.false;
		});

		it('is false for an empty path, so an unresolved item never matches', async () => {
			const isActive = await firstValueFrom(manager.isCurrentLocation(of('')));
			expect(isActive).to.be.false;
		});

		it('does not match a sibling path that shares a prefix', async () => {
			const isActive = await firstValueFrom(manager.isCurrentLocation(of(currentPath() + '-2')));
			expect(isActive).to.be.false;
		});

		it('re-evaluates when the path changes', async () => {
			const path = new UmbStringState('/some/other/path');
			const values: Array<boolean> = [];
			const subscription = manager.isCurrentLocation(path.asObservable()).subscribe((v) => values.push(v));

			path.setValue(currentPath());
			subscription.unsubscribe();

			expect(values).to.deep.equal([false, true]);
		});
	});

	describe('navigationend listener', () => {
		// This manager owns a listener on a global object, so it has to be bound to the host lifecycle.
		// A leak here is invisible until the tab has thousands of stale listeners.
		it('is added while the host is connected and removed again when it disconnects', async () => {
			let added = 0;
			let removed = 0;
			const originalAdd = window.addEventListener;
			const originalRemove = window.removeEventListener;
			window.addEventListener = function (this: Window, type: string, listener: any, options?: any) {
				if (type === 'navigationend') added++;
				return originalAdd.call(this, type, listener, options);
			} as typeof window.addEventListener;
			window.removeEventListener = function (this: Window, type: string, listener: any, options?: any) {
				if (type === 'navigationend') removed++;
				return originalRemove.call(this, type, listener, options);
			} as typeof window.removeEventListener;

			try {
				const hostElement = new UmbTestControllerHostElement();
				document.body.appendChild(hostElement);
				new UmbTreeItemActiveManager(hostElement);
				await aTimeout(0);

				expect(added).to.equal(1);
				expect(removed).to.equal(0);

				hostElement.remove();
				await aTimeout(0);

				expect(removed).to.equal(1);
			} finally {
				window.addEventListener = originalAdd;
				window.removeEventListener = originalRemove;
			}
		});
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has an activeTrail property', () => {
				expect(manager).to.have.property('activeTrail').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has an isActive method', () => {
				expect(manager).to.have.property('isActive').that.is.a('function');
			});
		});
	});

	describe('isActive', () => {
		it('checks if an item is active', (done) => {
			manager.setActiveTrail([item]);
			const isActive = manager.isActive(item);
			expect(isActive).to.be.an.instanceOf(Observable);
			manager.isActive(item).subscribe((value) => {
				expect(value).to.be.true;
				done();
			});
		});
	});

	describe('setActiveTrail & getActiveTrail', () => {
		it('sets and gets the active trail', () => {
			const trail = [item];
			manager.setActiveTrail(trail);
			expect(manager.getActiveTrail()).to.deep.equal(trail);
		});
	});

	describe('removeActiveTrailIfMatch', () => {
		it('removes the active trail', () => {
			const trail = [item];
			manager.setActiveTrail(trail);
			manager.removeActiveTrailIfMatch(trail);
			expect(manager.getActiveTrail()).to.deep.equal([]);
		});
		it('does not remove the active trail if it does not match', () => {
			const trail = [item];
			manager.setActiveTrail(trail);
			manager.removeActiveTrailIfMatch([item2]);
			expect(manager.getActiveTrail()).to.deep.equal([item]);
		});
	});

	describe('deprecated members', () => {
		it('setActive delegates to setActiveTrail', () => {
			manager.setActive([item]);
			expect(manager.getActiveTrail()).to.deep.equal([item]);
		});

		it('getActive delegates to getActiveTrail', () => {
			manager.setActiveTrail([item]);
			expect(manager.getActive()).to.deep.equal([item]);
		});

		it('removeActiveIfMatch delegates to removeActiveTrailIfMatch', () => {
			manager.setActiveTrail([item]);
			manager.removeActiveIfMatch([item]);
			expect(manager.getActiveTrail()).to.deep.equal([]);
		});

		it('active emits the same values as activeTrail', (done) => {
			manager.setActiveTrail([item]);
			manager.active.subscribe((value) => {
				expect(value).to.deep.equal([item]);
				done();
			});
		});
	});
});
