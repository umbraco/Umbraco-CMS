import { UmbContentTypeDesignEditorPropertyElement } from './content-type-design-editor-property.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbPropertyTypeModel } from '../../../types.js';

const OWNER_UNIQUE = 'owner-content-type';

class UmbTestStructureManager {
	#ownerContentType = new UmbObjectState<any>({ unique: OWNER_UNIQUE, name: 'Hero', isElement: true });
	#persistedProperty = new UmbObjectState<UmbPropertyTypeModel | undefined>(undefined);

	readonly ownerContentType = this.#ownerContentType.asObservable();

	ownerContentTypeObservablePart<R>(mappingFunction: (value: any) => R) {
		return this.#ownerContentType.asObservablePart(mappingFunction);
	}

	persistedPropertyById() {
		return this.#persistedProperty.asObservable();
	}

	getOwnerContentTypeUnique() {
		return OWNER_UNIQUE;
	}

	setIsElement(isElement: boolean) {
		this.#ownerContentType.update({ isElement });
	}

	setPersistedProperty(property: UmbPropertyTypeModel | undefined) {
		this.#persistedProperty.setValue(property);
	}
}

class UmbTestPropertyStructureHelper {
	readonly manager = new UmbTestStructureManager();

	getStructureManager() {
		return this.manager;
	}

	async contentTypeOfProperty() {
		return this.manager.ownerContentType;
	}
}

const propertyModel = (alias: string) =>
	({
		unique: 'property-unique',
		container: null,
		alias,
		name: 'Headline',
		description: '',
		dataType: {},
		variesByCulture: false,
		variesBySegment: false,
		sortOrder: 0,
		validation: { mandatory: false, mandatoryMessage: null, regEx: null, regExMessage: null },
		appearance: { labelOnTop: false },
	}) as unknown as UmbPropertyTypeModel;

describe('UmbContentTypeDesignEditorPropertyElement', () => {
	let element: UmbContentTypeDesignEditorPropertyElement;
	let helper: UmbTestPropertyStructureHelper;

	const setup = async (persistedAlias: string | undefined, currentAlias: string) => {
		helper = new UmbTestPropertyStructureHelper();
		helper.manager.setPersistedProperty(persistedAlias ? propertyModel(persistedAlias) : undefined);

		element = await fixture(html`<umb-content-type-design-editor-property></umb-content-type-design-editor-property>`);
		element.setAttribute('editpropertytypepath', '/edit/');
		element.propertyStructureHelper = helper as any;
		element.property = propertyModel(currentAlias);

		await aTimeout(0);
		await element.updateComplete;
	};

	const notice = () => element.shadowRoot!.querySelector('#alias-renamed-notice');

	it('warns when the alias of a stored property is changed on an Element Type', async () => {
		await setup('headline', 'title');
		expect(notice()).to.exist;
	});

	it('does not warn when the alias matches the stored one', async () => {
		await setup('headline', 'headline');
		expect(notice()).to.not.exist;
	});

	it('does not warn for a property that is not stored yet', async () => {
		await setup(undefined, 'title');
		expect(notice()).to.not.exist;
	});

	it('stops warning when the structure helper is taken away', async () => {
		await setup('headline', 'title');
		expect(notice()).to.exist;

		element.propertyStructureHelper = undefined;

		await aTimeout(0);
		await element.updateComplete;

		expect(notice()).to.not.exist;
	});

	it('does not warn when the owner is not an Element Type', async () => {
		await setup('headline', 'title');
		helper.manager.setIsElement(false);

		await aTimeout(0);
		await element.updateComplete;

		expect(notice()).to.not.exist;
	});
});
