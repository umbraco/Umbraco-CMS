import { UmbPaginationManager } from './pagination.manager.js';
import { expect, oneEvent } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

describe('UmbPaginationManager', () => {
	let manager: UmbPaginationManager;

	beforeEach(() => {
		manager = new UmbPaginationManager();
		manager.setPageSize(10);
		manager.setTotalItems(100);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a pageSize property', () => {
				expect(manager).to.have.property('pageSize').to.be.an.instanceOf(Observable);
			});

			it('has a totalItems property', () => {
				expect(manager).to.have.property('totalItems').to.be.an.instanceOf(Observable);
			});

			it('has a currentPage property', () => {
				expect(manager).to.have.property('currentPage').to.be.an.instanceOf(Observable);
			});

			it('has a skip property', () => {
				expect(manager).to.have.property('skip').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has a setPageSize method', () => {
				expect(manager).to.have.property('setPageSize').that.is.a('function');
			});

			it('has a getPageSize method', () => {
				expect(manager).to.have.property('getPageSize').that.is.a('function');
			});

			it('has a getTotalItems method', () => {
				expect(manager).to.have.property('getTotalItems').that.is.a('function');
			});

			it('has a getTotalPages method', () => {
				expect(manager).to.have.property('getTotalPages').that.is.a('function');
			});

			it('has a getCurrentPageNumber method', () => {
				expect(manager).to.have.property('getCurrentPageNumber').that.is.a('function');
			});

			it('has a setCurrentPageNumber method', () => {
				expect(manager).to.have.property('setCurrentPageNumber').that.is.a('function');
			});

			it('has a getSkip method', () => {
				expect(manager).to.have.property('getSkip').that.is.a('function');
			});
		});
	});

	describe('Page Size', () => {
		it('sets and gets the pageSize value', () => {
			manager.setPageSize(5);
			expect(manager.getPageSize()).to.equal(5);
		});

		it('updates the observable', (done) => {
			manager.setPageSize(2);

			manager.pageSize
				.subscribe((value) => {
					expect(value).to.equal(2);
					done();
				})
				.unsubscribe();
		});
	});

	describe('Total Items', () => {
		it('sets and gets the totalItems value', () => {
			manager.setTotalItems(200);
			expect(manager.getTotalItems()).to.equal(200);
		});

		it('updates the observable', (done) => {
			manager.totalItems
				.subscribe((value) => {
					expect(value).to.equal(100);
					done();
				})
				.unsubscribe();
		});

		it('recalculates the total pages', () => {
			expect(manager.getTotalPages()).to.equal(10);
		});

		it('it fall backs to the last page number if the totalPages is less than the currentPage', () => {
			manager.setCurrentPageNumber(10);
			manager.setTotalItems(20);
			expect(manager.getCurrentPageNumber()).to.equal(2);
		});
	});

	describe('Current Page', () => {
		it('sets and gets the currentPage value', () => {
			manager.setCurrentPageNumber(2);
			expect(manager.getCurrentPageNumber()).to.equal(2);
		});

		it('cant be set to a value less than 1', () => {
			manager.setCurrentPageNumber(0);
			expect(manager.getCurrentPageNumber()).to.equal(1);
		});

		it('cant be set to a value greater than the total pages', () => {
			manager.setPageSize(1);
			manager.setTotalItems(2);
			manager.setCurrentPageNumber(10);
			expect(manager.getCurrentPageNumber()).to.equal(2);
		});

		it('updates the observable', (done) => {
			manager.setCurrentPageNumber(2);

			manager.currentPage
				.subscribe((value) => {
					expect(value).to.equal(2);
					done();
				})
				.unsubscribe();
		});

		it('updates the skip value', () => {
			manager.setCurrentPageNumber(5);
			expect(manager.getSkip()).to.equal(40);
		});

		it('dispatches a change event', async () => {
			const listener = oneEvent(manager, UmbChangeEvent.TYPE);
			manager.setCurrentPageNumber(2);
			const event = (await listener) as unknown as UmbChangeEvent;
			const target = event.target as UmbPaginationManager;
			expect(event).to.exist;
			expect(event.type).to.equal(UmbChangeEvent.TYPE);
			expect(target.getCurrentPageNumber()).to.equal(2);
		});
	});

	describe('Skip', () => {
		it('gets the skip value', () => {
			manager.setCurrentPageNumber(5);
			expect(manager.getSkip()).to.equal(40);
		});

		it('updates the observable', (done) => {
			manager.setCurrentPageNumber(5);

			manager.skip
				.subscribe((value) => {
					expect(value).to.equal(40);
					done();
				})
				.unsubscribe();
		});
	});
});
