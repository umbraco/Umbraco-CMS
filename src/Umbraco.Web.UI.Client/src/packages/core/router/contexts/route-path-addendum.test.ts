import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbRoutePathAddendumResetContext } from './route-path-addendum-reset.context';
import { UmbRoutePathAddendumContext } from './route-path-addendum.context';

@customElement('umb-test-host')
class UmbTestHostElement extends UmbElementMixin(HTMLElement) {}

@customElement('umb-test-child')
class UmbTestChildElement extends UmbElementMixin(HTMLElement) {}

describe('UmbRoutepathAddendum', () => {
	let resetContext: UmbRoutePathAddendumResetContext;
	let addendumContext: UmbRoutePathAddendumContext;
	let host: UmbTestHostElement;
	let child: UmbTestChildElement;

	beforeEach(() => {
		host = new UmbTestHostElement();
		child = new UmbTestChildElement();
		host.appendChild(child);
		document.body.appendChild(host);
		resetContext = new UmbRoutePathAddendumResetContext(host);
		addendumContext = new UmbRoutePathAddendumContext(child);
	});

	afterEach(() => {
		document.body.removeChild(host);
		resetContext.destroy();
		addendumContext.destroy();
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a entity type property', () => {
				expect(addendumContext).to.have.property('addendum').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has a setAddendum method', () => {
				expect(addendumContext).to.have.property('setAddendum').that.is.a('function');
			});
		});
	});

	describe('build up addendum', () => {
		it('returns early set addendum', (done) => {
			addendumContext.setAddendum('hello/here');
			addendumContext.observe(addendumContext.addendum, (addendum) => {
				if (addendum) {
					expect(addendum).to.equal('hello/here');
					done();
				}
			});
		});

		it('returns late set addendum', (done) => {
			addendumContext.observe(addendumContext.addendum, (addendum) => {
				if (addendum) {
					expect(addendum).to.equal('hello/here');
					done();
				}
			});
			addendumContext.setAddendum('hello/here');
		});

		it('returns an updated addendum', (done) => {
			let count = 0;
			addendumContext.observe(addendumContext.addendum, (addendum) => {
				count++;
				if (count === 1) {
					if (addendum) {
						expect(addendum).to.equal('hello/here');
						done();
					}
				} else if (count === 2) {
					if (addendum) {
						expect(addendum).to.equal('hello/updated');
						done();
					}
				}
			});
			addendumContext.setAddendum('hello/here');
			addendumContext.setAddendum('hello/updated');
		});

		it('returns early set child addendum', (done) => {
			addendumContext.setAddendum('hello/here');
			const innerChild = new UmbTestChildElement();
			child.appendChild(innerChild);
			const childAddendumContext = new UmbRoutePathAddendumContext(innerChild);
			childAddendumContext.setAddendum('child-specification');

			childAddendumContext.observe(childAddendumContext.addendum, (addendum) => {
				if (addendum) {
					expect(addendum).to.equal('hello/here/child-specification');
					done();
				}
			});
		});

		it('returns late set child addendum', (done) => {
			const innerChild = new UmbTestChildElement();
			child.appendChild(innerChild);
			const childAddendumContext = new UmbRoutePathAddendumContext(innerChild);

			childAddendumContext.observe(childAddendumContext.addendum, (addendum) => {
				if (addendum) {
					expect(addendum).to.equal('hello/here/child-specification');
					done();
				}
			});

			childAddendumContext.setAddendum('child-specification');

			addendumContext.setAddendum('hello/here');
		});
	});

	it('work with empty string addendum', (done) => {
		addendumContext.setAddendum('hello/here');
		const innerChild = new UmbTestChildElement();
		child.appendChild(innerChild);
		const childAddendumContext = new UmbRoutePathAddendumContext(innerChild);
		childAddendumContext.setAddendum('');

		childAddendumContext.observe(childAddendumContext.addendum, (addendum) => {
			if (addendum) {
				expect(addendum).to.equal('hello/here');
				done();
			}
		});
	});
});
