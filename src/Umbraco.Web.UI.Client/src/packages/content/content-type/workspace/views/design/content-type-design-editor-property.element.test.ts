import { UmbContentTypeDesignEditorPropertyElement } from './content-type-design-editor-property.element.js';
import { aTimeout, expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
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

class UmbTestModalManagerContext {
	readonly opened: Array<{ headline?: string; args?: unknown[] }> = [];

	#host: UmbLitElement;
	#confirm = true;

	constructor(host: UmbLitElement) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host;
	}

	rejectNext() {
		this.#confirm = false;
	}

	open(_host: unknown, _alias: unknown, args: any) {
		// The message is a lit template whose single binding is the localization arguments.
		this.opened.push({ headline: args?.data?.headline, args: args?.data?.content?.values?.[0] });
		return {
			onSubmit: () => (this.#confirm ? Promise.resolve(undefined) : Promise.reject(new Error('cancelled'))),
		};
	}
}

class UmbTestPropertyStructureHelper {
	readonly updates: Array<Partial<UmbPropertyTypeModel>> = [];

	getStructureManager() {
		return {
			getOwnerContentTypeUnique: () => OWNER_UNIQUE,
		};
	}

	async contentTypeOfProperty() {
		return new UmbObjectState({ unique: OWNER_UNIQUE, name: 'Hero' }).asObservable();
	}

	partialUpdateProperty(_unique: string, partial: Partial<UmbPropertyTypeModel>) {
		this.updates.push(partial);
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
	let modalManager: UmbTestModalManagerContext;
	let structureHelper: UmbTestPropertyStructureHelper;

	const setup = async (persisted: UmbContentTypeDetailModel | undefined, currentAlias: string) => {
		const host: UmbTestPropertyHostElement = await fixture(
			html`<umb-test-content-type-design-editor-property-host>
				<umb-content-type-design-editor-property></umb-content-type-design-editor-property>
			</umb-test-content-type-design-editor-property-host>`,
		);

		const workspaceContext = new UmbTestWorkspaceContext(host);
		workspaceContext.setPersisted(persisted);
		host.provideContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, workspaceContext as never);

		modalManager = new UmbTestModalManagerContext(host);
		host.provideContext(UMB_MODAL_MANAGER_CONTEXT, modalManager as never);

		structureHelper = new UmbTestPropertyStructureHelper();

		element = host.querySelector('umb-content-type-design-editor-property')!;
		element.setAttribute('editpropertytypepath', '/edit/');
		element.propertyStructureHelper = structureHelper as never;
		element.property = propertyModel(currentAlias);

		await aTimeout(0);
		await element.updateComplete;
	};

	const finishEditingAlias = async () => {
		element
			.shadowRoot!.querySelector('umb-input-with-alias')!
			.dispatchEvent(new FocusEvent('focusout', { bubbles: true, composed: true }));
		await aTimeout(0);
		await element.updateComplete;
	};

	it('asks for confirmation when the alias of a stored property is changed on an Element Type', async () => {
		await setup(contentTypeModel(true, [propertyModel('headline')]), 'title');
		await finishEditingAlias();

		expect(modalManager.opened.length).to.equal(1);
		expect(modalManager.opened[0].args).to.deep.equal(['Headline', 'headline']);
	});

	it('does not ask when the alias matches the stored one', async () => {
		await setup(contentTypeModel(true, [propertyModel('headline')]), 'headline');
		await finishEditingAlias();

		expect(modalManager.opened.length).to.equal(0);
	});

	it('does not ask for a property that is not stored yet', async () => {
		await setup(contentTypeModel(true, []), 'title');
		await finishEditingAlias();

		expect(modalManager.opened.length).to.equal(0);
	});

	it('does not ask when the owner is not an Element Type', async () => {
		await setup(contentTypeModel(false, [propertyModel('headline')]), 'title');
		await finishEditingAlias();

		expect(modalManager.opened.length).to.equal(0);
	});

	it('does not ask again once the change is confirmed', async () => {
		await setup(contentTypeModel(true, [propertyModel('headline')]), 'title');
		await finishEditingAlias();
		await finishEditingAlias();

		expect(modalManager.opened.length).to.equal(1);
	});

	it('restores the stored alias when the change is not confirmed', async () => {
		await setup(contentTypeModel(true, [propertyModel('headline')]), 'title');
		modalManager.rejectNext();
		await finishEditingAlias();

		expect(structureHelper.updates).to.deep.equal([{ alias: 'headline' }]);
	});
});
