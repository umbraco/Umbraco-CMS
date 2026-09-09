import { UmbEntryWorkspaceDataManager } from './entry-data-manager.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntryDetailModel } from '../types.js';

@customElement('test-element-data-manager-host')
class UmbTestElementDataManagerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbEntryWorkspaceDataManager', () => {
	let manager: UmbEntryWorkspaceDataManager<UmbEntryDetailModel>;

	const newPropertyValue = { alias: 'test', culture: null, segment: null, editorAlias: 'test' };

	beforeEach(() => {
		const hostElement = new UmbTestElementDataManagerHostElement();
		manager = new UmbEntryWorkspaceDataManager(hostElement);
		manager.setCurrent({ values: [] });
	});

	describe('initiatePropertyValueChange and finishPropertyValueChange', () => {
		it('suppresses observer emissions while initiated', () => {
			const emissions: Array<UmbEntryDetailModel | undefined> = [];
			const subscription = manager.current.subscribe((value) => emissions.push(value));

			manager.initiatePropertyValueChange();
			manager.updateCurrent({ values: [newPropertyValue] });

			expect(emissions.length).to.equal(1);
			expect(emissions[0]?.values.length).to.equal(0);

			subscription.unsubscribe();
		});

		it('flushes buffered changes to observers when finish is called', () => {
			const emissions: Array<UmbEntryDetailModel | undefined> = [];
			const subscription = manager.current.subscribe((value) => emissions.push(value));

			manager.initiatePropertyValueChange();
			manager.updateCurrent({ values: [newPropertyValue] });
			manager.finishPropertyValueChange();

			expect(emissions.length).to.equal(2);
			expect(emissions[1]?.values.length).to.equal(1);

			subscription.unsubscribe();
		});

		it('does not emit until all nested initiates are finished', () => {
			const emissions: Array<UmbEntryDetailModel | undefined> = [];
			const subscription = manager.current.subscribe((value) => emissions.push(value));

			manager.initiatePropertyValueChange();
			manager.initiatePropertyValueChange();
			manager.updateCurrent({ values: [newPropertyValue] });

			manager.finishPropertyValueChange();
			expect(emissions.length).to.equal(1);

			manager.finishPropertyValueChange();
			expect(emissions.length).to.equal(2);
			expect(emissions[1]?.values.length).to.equal(1);

			subscription.unsubscribe();
		});
	});

	describe('values ordering (_sortCurrentData)', () => {
		const valueA = { alias: 'a', culture: null, segment: null, editorAlias: 'test', value: 'A' };
		const valueB = { alias: 'b', culture: null, segment: null, editorAlias: 'test', value: 'B' };
		const valueC = { alias: 'c', culture: null, segment: null, editorAlias: 'test', value: 'C' };

		it('sorts current values to match persisted order when current is set', () => {
			manager.setPersisted({ values: [valueA, valueB] });
			manager.setCurrent({ values: [valueB, valueA] });
			expect(manager.getCurrent()?.values.map((x) => x.alias)).to.deep.equal(['a', 'b']);
		});

		it('re-sorts existing current values when persisted is set again with a different order', () => {
			manager.setCurrent({ values: [valueB, valueA] });
			manager.setPersisted({ values: [valueA, valueB] });
			expect(manager.getCurrent()?.values.map((x) => x.alias)).to.deep.equal(['a', 'b']);
			expect(manager.getHasUnpersistedChanges()).to.be.false;
		});

		it('re-sorts existing current values when persisted is updated with a different order', () => {
			manager.setPersisted({ values: [valueB, valueA] });
			manager.setCurrent({ values: [valueB, valueA] });
			manager.updatePersisted({ values: [valueA, valueB] });
			expect(manager.getCurrent()?.values.map((x) => x.alias)).to.deep.equal(['a', 'b']);
			expect(manager.getHasUnpersistedChanges()).to.be.false;
		});

		it('appends a current-only value after every value known to persisted', () => {
			manager.setPersisted({ values: [valueA, valueB] });
			manager.setCurrent({ values: [valueC, valueB, valueA] });
			expect(manager.getCurrent()?.values.map((x) => x.alias)).to.deep.equal(['a', 'b', 'c']);
		});
	});

	describe('destroy', () => {
		it('does not throw when finishPropertyValueChange is called after destroy', () => {
			manager.initiatePropertyValueChange();
			manager.destroy();

			expect(() => manager.finishPropertyValueChange()).to.not.throw();
		});
	});
});
