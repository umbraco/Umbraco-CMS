import { expect, oneEvent } from '@open-wc/testing';
import { UmbPaginationManager } from './pagination.manager.js';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event'

describe('UmbContextConsumer', () => {
	let manager: UmbPaginationManager;

	beforeEach(() => {
		manager = new UmbPaginationManager();
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
      manager.setPageSize(10);
      expect(manager.getPageSize()).to.equal(10);
    });

    it('updates the observable', (done) => {
      manager.setPageSize(2);
      
      manager.pageSize.subscribe((value) => {
        expect(value).to.equal(2);
        done();
      })
      .unsubscribe();
    });
  });

  describe('Total Items', () => {
    it('sets and gets the totalItems value', () => {
      manager.setTotalItems(100);
      expect(manager.getTotalItems()).to.equal(100);
    });

    it('updates the observable', (done) => {
      manager.setTotalItems(100);
      
      manager.totalItems.subscribe((value) => {
        expect(value).to.equal(100);
        done();
      })
      .unsubscribe();
    });

    it('recalculates the total pages', () => {
      manager.setPageSize(5);
      manager.setTotalItems(100);
      expect(manager.getTotalPages()).to.equal(20);
    });
  });

  describe('Current Page', () => {
    it('sets and gets the currentPage value', () => {
      manager.setCurrentPageNumber(2);
      expect(manager.getCurrentPageNumber()).to.equal(2);
    });

    it('updates the observable', (done) => {
      manager.setCurrentPageNumber(2);
      
      manager.currentPage.subscribe((value) => {
        expect(value).to.equal(2);
        done();
      })
      .unsubscribe();
    });

    it('updates the skip value', () => {
      manager.setPageSize(5);
      manager.setTotalItems(100);
      manager.setCurrentPageNumber(5);
      expect(manager.getSkip()).to.equal(20);
    });

    it('dispatches a change event', async () => {
      const listener = oneEvent(manager, UmbChangeEvent.TYPE);
      manager.setCurrentPageNumber(200);
      const event = (await listener) as unknown as UmbChangeEvent;
      expect(event).to.exist;
      expect(event.type).to.equal(UmbChangeEvent.TYPE);
      expect(event.target.getCurrentPageNumber()).to.equal(200);
    });
  });

  describe('Skip', () => {
    it('gets the skip value', () => {
      manager.setPageSize(5);
      manager.setTotalItems(100);
      manager.setCurrentPageNumber(5);
      expect(manager.getSkip()).to.equal(20);
    });

    it('updates the observable', (done) => {
      manager.setPageSize(5);
      manager.setTotalItems(100);
      manager.setCurrentPageNumber(5);
      
      manager.skip.subscribe((value) => {
        expect(value).to.equal(20);
        done();
      })
      .unsubscribe();
    });
  });
});
