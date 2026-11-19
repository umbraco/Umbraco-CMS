import { UmbContentCollectionConfigurationContext } from './content-collection-configuration.context.js';
import { umbMapDataTypeToCollectionConfiguration } from './map-data-type-to-collection-configuration.function.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbDataTypeDetailModel } from '@umbraco-cms/backoffice/data-type';

@customElement('test-content-collection-configuration-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const readOnce = <T>(observable: Observable<T>): T => {
	let value!: T;
	observable.subscribe((emitted) => (value = emitted)).unsubscribe();
	return value;
};

function makeDataType(values: UmbDataTypeDetailModel['values'] = []): UmbDataTypeDetailModel {
	return {
		entityType: 'data-type',
		unique: 'data-type-unique',
		name: 'Test Collection',
		editorAlias: 'Umbraco.ListView',
		editorUiAlias: 'Umb.PropertyEditorUi.Collection',
		values,
	};
}

describe('umbMapDataTypeToCollectionConfiguration', () => {
	it('maps the parts of the configuration the data type describes', () => {
		const config = umbMapDataTypeToCollectionConfiguration(
			makeDataType([
				{ alias: 'pageSize', value: 25 },
				{ alias: 'orderBy', value: 'name' },
				{ alias: 'orderDirection', value: 'desc' },
			]),
		);

		expect(config.pageSize).to.equal(25);
		expect(config.orderBy).to.equal('name');
		expect(config.orderDirection).to.equal('desc');
	});

	it('falls back when the data type says nothing about ordering or page size', () => {
		const config = umbMapDataTypeToCollectionConfiguration(makeDataType());

		expect(config.orderBy).to.equal('updateDate');
		expect(config.orderDirection).to.equal('asc');
		expect(config.pageSize).to.equal(50);
	});

	// `dataTypeId` reaches the server as `dataTypeKey`, which only resolves a collection configured as a content type
	// *property*. Supplying it for a collection configured on the content type itself makes the request fail, so it must
	// stay absent no matter what the data type holds.
	it('never sets dataTypeId', () => {
		const config = umbMapDataTypeToCollectionConfiguration(makeDataType([{ alias: 'pageSize', value: 25 }]));

		expect(config).to.not.have.property('dataTypeId');
	});

	it('does not decide which entity the collection is scoped to', () => {
		const config = umbMapDataTypeToCollectionConfiguration(makeDataType());

		expect(config).to.not.have.property('unique');
	});
});

describe('UmbContentCollectionConfigurationContext', () => {
	let hostElement: UmbTestControllerHostElement;
	let context: UmbContentCollectionConfigurationContext;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		context = new UmbContentCollectionConfigurationContext(hostElement);
	});

	describe('the tri-state', () => {
		it('has no collection concept when no alias is given', () => {
			context.setDataTypeUnique('some-data-type');

			expect(readOnce(context.hasCollection)).to.be.false;
			expect(context.getHasCollection()).to.be.false;
		});

		it('has a collection concept but none configured when only an alias is given', () => {
			context.setCollectionAlias('Umb.Collection.Document');

			expect(readOnce(context.collectionAlias)).to.equal('Umb.Collection.Document');
			expect(readOnce(context.dataTypeUnique)).to.be.undefined;
			expect(readOnce(context.hasCollection)).to.be.false;
		});

		it('has a collection once both an alias and a configuring data type are known', () => {
			context.setCollectionAlias('Umb.Collection.Document');
			context.setDataTypeUnique('some-data-type');

			expect(readOnce(context.hasCollection)).to.be.true;
			expect(context.getHasCollection()).to.be.true;
		});

		it('loses the collection again when the data type is taken away', () => {
			context.setCollectionAlias('Umb.Collection.Document');
			context.setDataTypeUnique('some-data-type');
			context.setDataTypeUnique(undefined);

			expect(readOnce(context.hasCollection)).to.be.false;
		});
	});

	describe('the subject', () => {
		it('distinguishes the root from having no subject at all', () => {
			expect(readOnce(context.unique)).to.be.undefined;

			context.setUnique(null);

			expect(readOnce(context.unique)).to.be.null;
			expect(context.getUnique()).to.be.null;
		});

		it('re-points at another entity', () => {
			context.setUnique('first');
			context.setUnique('second');

			expect(context.getUnique()).to.equal('second');
		});
	});

	describe('the resolved configuration', () => {
		it('is undefined when no collection is configured', () => {
			context.setCollectionAlias('Umb.Collection.Document');
			context.setUnique('some-entity');

			expect(readOnce(context.collectionConfig)).to.be.undefined;
		});

		it('is undefined while the data type has not resolved', () => {
			context.setCollectionAlias('Umb.Collection.Document');
			context.setDataTypeUnique('a-data-type-that-does-not-exist');
			context.setUnique('some-entity');

			expect(readOnce(context.collectionConfig)).to.be.undefined;
			expect(context.getDataType()).to.be.undefined;
		});
	});
});
