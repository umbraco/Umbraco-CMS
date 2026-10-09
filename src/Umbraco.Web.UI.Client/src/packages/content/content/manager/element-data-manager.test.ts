import { UmbElementWorkspaceDataManager } from './element-data-manager.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbElementDetailModel } from '../types.js';

@customElement('test-element-data-manager-host')
class UmbTestElementDataManagerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbElementWorkspaceDataManager', () => {
	let manager: UmbElementWorkspaceDataManager<UmbElementDetailModel>;

	const newPropertyValue = { alias: 'test', culture: null, segment: null, editorAlias: 'test' };

	beforeEach(() => {
		const hostElement = new UmbTestElementDataManagerHostElement();
		manager = new UmbElementWorkspaceDataManager(hostElement);
		manager.setCurrent({ values: [] });
	});

	describe('initiatePropertyValueChange and finishPropertyValueChange', () => {
		it('suppresses observer emissions while initiated', () => {
			const emissions: Array<UmbElementDetailModel | undefined> = [];
			const subscription = manager.current.subscribe((value) => emissions.push(value));

			manager.initiatePropertyValueChange();
			manager.updateCurrent({ values: [newPropertyValue] });

			expect(emissions.length).to.equal(1);
			expect(emissions[0]?.values.length).to.equal(0);

			subscription.unsubscribe();
		});

		it('flushes buffered changes to observers when finish is called', () => {
			const emissions: Array<UmbElementDetailModel | undefined> = [];
			const subscription = manager.current.subscribe((value) => emissions.push(value));

			manager.initiatePropertyValueChange();
			manager.updateCurrent({ values: [newPropertyValue] });
			manager.finishPropertyValueChange();

			expect(emissions.length).to.equal(2);
			expect(emissions[1]?.values.length).to.equal(1);

			subscription.unsubscribe();
		});

		it('does not emit until all nested initiates are finished', () => {
			const emissions: Array<UmbElementDetailModel | undefined> = [];
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

	describe('destroy', () => {
		it('does not throw when finishPropertyValueChange is called after destroy', () => {
			manager.initiatePropertyValueChange();
			manager.destroy();

			expect(() => manager.finishPropertyValueChange()).to.not.throw();
		});
	});
});
