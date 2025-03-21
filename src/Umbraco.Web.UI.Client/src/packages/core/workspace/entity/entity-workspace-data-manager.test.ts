import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbEntityWorkspaceDataManager } from './entity-workspace-data-manager.js';
import { UmbControllerHostElementElement } from '@umbraco-cms/backoffice/controller-api';

interface TestDataModel {
	name: string;
}

describe('UmbEntityWorkspaceDataManager', () => {
	let manager: UmbEntityWorkspaceDataManager<TestDataModel>;

	beforeEach(() => {
		const hostElement = new UmbControllerHostElementElement();
		manager = new UmbEntityWorkspaceDataManager(hostElement);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a persisted property', () => {
				expect(manager).to.have.property('persisted').to.be.an.instanceOf(Observable);
			});

			it('has a current property', () => {
				expect(manager).to.have.property('current').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has a getPersisted method', () => {
				expect(manager).to.have.property('getPersisted').that.is.a('function');
			});

			it('has a setPersisted method', () => {
				expect(manager).to.have.property('setPersisted').that.is.a('function');
			});

			it('has a updatePersisted method', () => {
				expect(manager).to.have.property('updatePersisted').that.is.a('function');
			});

			it('has a createObservablePartOfPersisted method', () => {
				expect(manager).to.have.property('createObservablePartOfPersisted').that.is.a('function');
			});

			it('has a getCurrent method', () => {
				expect(manager).to.have.property('getCurrent').that.is.a('function');
			});

			it('has a setCurrent method', () => {
				expect(manager).to.have.property('setCurrent').that.is.a('function');
			});

			it('has a updateCurrent method', () => {
				expect(manager).to.have.property('updateCurrent').that.is.a('function');
			});

			it('has a createObservablePartOfCurrent method', () => {
				expect(manager).to.have.property('createObservablePartOfCurrent').that.is.a('function');
			});

			it('has a getHasUnpersistedChanges method', () => {
				expect(manager).to.have.property('getHasUnpersistedChanges').that.is.a('function');
			});

			it('has a resetCurrent method', () => {
				expect(manager).to.have.property('resetCurrent').that.is.a('function');
			});

			it('has a clear method', () => {
				expect(manager).to.have.property('clear').that.is.a('function');
			});

			it('has a destroy method', () => {
				expect(manager).to.have.property('destroy').that.is.a('function');
			});
		});
	});

	describe('getPersisted and setPersisted', () => {
		it('returns the persisted data', () => {
			const data = { name: 'test' };
			manager.setPersisted(data);
			expect(manager.getPersisted()).to.deep.equal(data);
		});
	});

	describe('updatePersisted', () => {
		it('updates the persisted data', () => {
			const data = { name: 'test' };
			manager.setPersisted(data);
			manager.updatePersisted({ name: 'updated' });
			expect(manager.getPersisted()).to.deep.equal({ name: 'updated' });
		});
	});

	describe('createObservablePartOfPersisted', () => {
		it('creates an observable part of the persisted data', (done) => {
			const data = { name: 'test' };
			manager.setPersisted(data);
			const observablePart = manager.createObservablePartOfPersisted((data) => data?.name);

			observablePart.subscribe((value) => {
				expect(value).to.equal('test');
				done();
			});
		});
	});

	describe('getCurrent and setCurrent', () => {
		it('returns the current data', () => {
			const data = { name: 'test' };
			manager.setCurrent(data);
			expect(manager.getCurrent()).to.deep.equal(data);
		});
	});

	describe('updateCurrent', () => {
		it('updates the current data', () => {
			const data = { name: 'test' };
			manager.setCurrent(data);
			manager.updateCurrent({ name: 'updated' });
			expect(manager.getCurrent()).to.deep.equal({ name: 'updated' });
		});
	});

	describe('createObservablePartOfCurrent', () => {
		it('creates an observable part of the current data', (done) => {
			const data = { name: 'test' };
			manager.setCurrent(data);
			const observablePart = manager.createObservablePartOfCurrent((data) => data?.name);

			observablePart.subscribe((value) => {
				expect(value).to.equal('test');
				done();
			});
		});
	});

	describe('getHasUnpersistedChanges', () => {
		beforeEach(() => {
			const data = { name: 'test' };
			manager.setPersisted(data);
			manager.setCurrent(data);
		});

		it('returns false when current and persisted data are the same', () => {
			expect(manager.getHasUnpersistedChanges()).to.be.false;
		});

		it('returns true when current and persisted data are different', () => {
			manager.setCurrent({ name: 'updated' });
			expect(manager.getHasUnpersistedChanges()).to.be.true;
		});
	});

	describe('resetCurrent', () => {
		it('resets the current data to the persisted data', () => {
			const data = { name: 'test' };
			manager.setPersisted(data);
			manager.setCurrent({ name: 'updated' });
			manager.resetCurrent();
			expect(manager.getCurrent()).to.deep.equal(data);
		});
	});

	describe('clear', () => {
		it('clears the current data', () => {
			const data = { name: 'test' };
			manager.setPersisted(data);
			manager.setCurrent(data);
			manager.clear();
			expect(manager.getCurrent()).to.be.undefined;
			expect(manager.getPersisted()).to.be.undefined;
		});
	});
});
