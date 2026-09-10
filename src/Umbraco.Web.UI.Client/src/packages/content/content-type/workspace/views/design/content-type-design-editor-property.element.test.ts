import { UmbContentTypeDesignEditorPropertyElement } from './content-type-design-editor-property.element.js';
import { aTimeout, expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbContentTypeDetailModel, UmbPropertyTypeModel } from '../../../types.js';

const OWNER_UNIQUE = 'owner-content-type';
const PROPERTY_UNIQUE = 'property-unique';

@customElement('umb-test-content-type-design-editor-property-host')
class UmbTestPropertyHostElement extends UmbLitElement {
	override render() {
		return html`<slot></slot>`;
	}
}

class UmbTestWorkspaceContext {
	readonly IS_ENTITY_DETAIL_WORKSPACE_CONTEXT = true;

	#host: UmbLitElement;
	#persisted = new UmbObjectState<UmbContentTypeDetailModel | undefined>(undefined);
	readonly persistedData = this.#persisted.asObservable();

	constructor(host: UmbLitElement) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host;
	}

	setPersisted(data: UmbContentTypeDetailModel | undefined) {
		this.#persisted.setValue(data);
	}
}

class UmbTestPropertyStructureHelper {
	getStructureManager() {
		return {
			getOwnerContentTypeUnique: () => OWNER_UNIQUE,
		};
	}

	async contentTypeOfProperty() {
		return new UmbObjectState({ unique: OWNER_UNIQUE, name: 'Hero' }).asObservable();
	}
}

const propertyModel = (alias: string) =>
	({
		unique: PROPERTY_UNIQUE,
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

const contentTypeModel = (isElement: boolean, properties: Array<UmbPropertyTypeModel>) =>
	({
		unique: OWNER_UNIQUE,
		name: 'Hero',
		alias: 'hero',
		isElement,
		properties,
	}) as unknown as UmbContentTypeDetailModel;

describe('UmbContentTypeDesignEditorPropertyElement', () => {
	let element: UmbContentTypeDesignEditorPropertyElement;
	let workspaceContext: UmbTestWorkspaceContext;

	const setup = async (persisted: UmbContentTypeDetailModel | undefined, currentAlias: string) => {
		const host: UmbTestPropertyHostElement = await fixture(
			html`<umb-test-content-type-design-editor-property-host>
				<umb-content-type-design-editor-property></umb-content-type-design-editor-property>
			</umb-test-content-type-design-editor-property-host>`,
		);

		workspaceContext = new UmbTestWorkspaceContext(host);
		workspaceContext.setPersisted(persisted);
		host.provideContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, workspaceContext as never);

		element = host.querySelector('umb-content-type-design-editor-property')!;
		element.setAttribute('editpropertytypepath', '/edit/');
		element.propertyStructureHelper = new UmbTestPropertyStructureHelper() as never;
		element.property = propertyModel(currentAlias);

		await aTimeout(0);
		await element.updateComplete;
	};

	const notice = () => element.shadowRoot!.querySelector('#alias-renamed-notice');

	it('warns when the alias of a stored property is changed on an Element Type', async () => {
		await setup(contentTypeModel(true, [propertyModel('headline')]), 'title');
		expect(notice()).to.exist;
	});

	it('does not warn when the alias matches the stored one', async () => {
		await setup(contentTypeModel(true, [propertyModel('headline')]), 'headline');
		expect(notice()).to.not.exist;
	});

	it('does not warn for a property that is not stored yet', async () => {
		await setup(contentTypeModel(true, []), 'title');
		expect(notice()).to.not.exist;
	});

	it('does not warn when the owner is not an Element Type', async () => {
		await setup(contentTypeModel(false, [propertyModel('headline')]), 'title');
		expect(notice()).to.not.exist;
	});

	it('stops warning once the rename is stored', async () => {
		await setup(contentTypeModel(true, [propertyModel('headline')]), 'title');
		expect(notice()).to.exist;

		workspaceContext.setPersisted(contentTypeModel(true, [propertyModel('title')]));

		await aTimeout(0);
		await element.updateComplete;

		expect(notice()).to.not.exist;
	});
});
