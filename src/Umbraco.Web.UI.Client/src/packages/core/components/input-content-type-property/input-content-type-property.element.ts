import { UmbDocumentTypePickerContext } from '../../../documents/document-types/components/input-document-type/input-document-type.context.js';
import { UmbMediaTypePickerContext } from '../../../media/media-types/components/input-media-type/input-media-type.context.js';
import { UmbMemberTypePickerContext } from '../../../members/member-type/components/input-member-type/input-member-type.context.js';
import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbMediaTypeDetailRepository } from '@umbraco-cms/backoffice/media-type';
import { UmbMemberTypeDetailRepository } from '@umbraco-cms/backoffice/member-type';
import { UMB_ITEM_PICKER_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbItemPickerModel } from '@umbraco-cms/backoffice/modal';
import type { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';

export type UmbContentTypePropertyValue = {
	label: string;
	alias: string;
	isSystem: boolean;
};

type UmbInputContentTypePropertyConfigurationItem = {
	item: UmbItemPickerModel;
	pickerContext(): UmbPickerInputContext<any>;
	pickableFilter?(item: any): boolean;
	repository(): UmbDetailRepositoryBase<any>;
	systemProperties?: Array<UmbItemPickerModel>;
};

type UmbInputContentTypePropertyConfiguration = {
	documentTypes: UmbInputContentTypePropertyConfigurationItem;
	elementTypes: UmbInputContentTypePropertyConfigurationItem;
	mediaTypes: UmbInputContentTypePropertyConfigurationItem;
	memberTypes: UmbInputContentTypePropertyConfigurationItem;
};

@customElement('umb-input-content-type-property')
export class UmbInputContentTypePropertyElement extends FormControlMixin(UmbLitElement) {
	#configuration: UmbInputContentTypePropertyConfiguration = {
		documentTypes: {
			item: {
				label: this.localize.term('content_documentType'),
				description: this.localize.term('defaultdialogs_selectContentType'),
				value: 'documentTypes',
			},
			pickerContext: () => new UmbDocumentTypePickerContext(this),
			pickableFilter: (docType) => !docType.isElement,
			repository: () => new UmbDocumentTypeDetailRepository(this),
			systemProperties: [
				{
					label: this.localize.term('content_documentType'),
					description: 'contentTypeAlias',
					value: 'contentTypeAlias',
				},
				{ label: this.localize.term('content_createDate'), description: 'createDate', value: 'createDate' },
				{ label: this.localize.term('content_createBy'), description: 'owner', value: 'owner' },
				{ label: this.localize.term('content_isPublished'), description: 'published', value: 'published' },
				{ label: this.localize.term('general_sort'), description: 'sortOrder', value: 'sortOrder' },
				{ label: this.localize.term('content_updateDate'), description: 'updateDate', value: 'updateDate' },
				{ label: this.localize.term('content_updatedBy'), description: 'updater', value: 'updater' },
			],
		},
		elementTypes: {
			item: {
				label: this.localize.term('create_elementType'),
				description: this.localize.term('content_nestedContentSelectElementTypeModalTitle'),
				value: 'elementTypes',
			},
			pickerContext: () => new UmbDocumentTypePickerContext(this),
			pickableFilter: (docType) => docType.isElement,
			repository: () => new UmbDocumentTypeDetailRepository(this),
			systemProperties: [
				{
					label: this.localize.term('content_documentType'),
					description: 'contentTypeAlias',
					value: 'contentTypeAlias',
				},
			],
		},
		mediaTypes: {
			item: {
				label: this.localize.term('content_mediatype'),
				description: this.localize.term('defaultdialogs_selectMediaType'),
				value: 'mediaTypes',
			},
			pickerContext: () => new UmbMediaTypePickerContext(this),
			repository: () => new UmbMediaTypeDetailRepository(this),
			systemProperties: [
				{
					label: this.localize.term('content_documentType'),
					description: 'contentTypeAlias',
					value: 'contentTypeAlias',
				},
				{ label: this.localize.term('content_createDate'), description: 'createDate', value: 'createDate' },
				{ label: this.localize.term('content_createBy'), description: 'owner', value: 'owner' },
				{ label: this.localize.term('general_sort'), description: 'sortOrder', value: 'sortOrder' },
				{ label: this.localize.term('content_updateDate'), description: 'updateDate', value: 'updateDate' },
				{ label: this.localize.term('content_updatedBy'), description: 'updater', value: 'updater' },
			],
		},
		memberTypes: {
			item: {
				label: this.localize.term('content_membertype'),
				description: this.localize.term('defaultdialogs_selectMemberType'),
				value: 'memberTypes',
			},
			pickerContext: () => new UmbMemberTypePickerContext(this),
			repository: () => new UmbMemberTypeDetailRepository(this),
			systemProperties: [
				{
					label: this.localize.term('content_documentType'),
					description: 'contentTypeAlias',
					value: 'contentTypeAlias',
				},
				{ label: this.localize.term('content_createDate'), description: 'createDate', value: 'createDate' },
				{ label: this.localize.term('general_email'), description: 'email', value: 'email' },
				{ label: this.localize.term('content_updateDate'), description: 'updateDate', value: 'updateDate' },
				{ label: this.localize.term('general_username'), description: 'username', value: 'username' },
			],
		},
	};

	#modalManager?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;

	protected getFormElement() {
		return undefined;
	}

	public selectedProperty?: UmbContentTypePropertyValue;

	@property({ type: Boolean, attribute: 'document-types' })
	public documentTypes: boolean = false;

	@property({ type: Boolean, attribute: 'element-types' })
	public elementTypes: boolean = false;

	@property({ type: Boolean, attribute: 'media-types' })
	public mediaTypes: boolean = false;

	@property({ type: Boolean, attribute: 'member-types' })
	public memberTypes: boolean = false;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (modalManager) => {
			this.#modalManager = modalManager;
		});
	}

	async #onClick() {
		const items: Array<UmbItemPickerModel> = [];

		if (this.documentTypes) {
			items.push(this.#configuration['documentTypes'].item);
		}

		if (this.elementTypes) {
			items.push(this.#configuration['elementTypes'].item);
		}

		if (this.mediaTypes) {
			items.push(this.#configuration['mediaTypes'].item);
		}

		if (this.memberTypes) {
			items.push(this.#configuration['memberTypes'].item);
		}

		if (items.length === 1) {
			// If there is only one item, we can skip the modal and go directly to the picker.
			this.#openContentTypePicker(items[0].value as keyof UmbInputContentTypePropertyConfiguration);
			return;
		}

		if (!this.#modalManager) return;

		const modalContext = this.#modalManager.open(this, UMB_ITEM_PICKER_MODAL, {
			data: {
				headline: this.localize.term('defaultdialogs_selectContentType'),
				items: items,
			},
		});

		const modalValue = await modalContext.onSubmit();

		if (!modalValue) return;

		const configKey = modalValue.value as keyof UmbInputContentTypePropertyConfiguration;
		this.#openContentTypePicker(configKey);
	}

	async #openContentTypePicker(configKey: keyof UmbInputContentTypePropertyConfiguration) {
		const config = this.#configuration[configKey];
		if (!config) return;

		const pickerContext = config.pickerContext();

		pickerContext.max = 1;

		await pickerContext.openPicker({
			hideTreeRoot: true,
			multiple: false,
			pickableFilter: config.pickableFilter,
		});

		const selectedItems = pickerContext.getSelection();
		if (selectedItems.length === 0) return;

		const repository = config.repository();
		const { data } = await repository.requestByUnique(selectedItems[0]);

		if (!data) return;

		this.#openPropertyPicker(data, config.systemProperties);
	}

	async #openPropertyPicker(contentType?: UmbContentTypeModel, systemProperties?: Array<UmbItemPickerModel>) {
		if (!contentType) return;
		if (!this.#modalManager) return;

		const properties: Array<UmbItemPickerModel> =
			contentType?.properties.map((property) => ({
				label: property.name,
				value: property.alias,
				description: property.alias,
			})) ?? [];

		const items = [...(systemProperties ?? []), ...properties];

		const modalContext = this.#modalManager.open(this, UMB_ITEM_PICKER_MODAL, {
			data: {
				headline: `Select a property from ${contentType.name}`,
				items: items,
			},
		});

		const modalValue = await modalContext.onSubmit();

		if (!modalValue) return;

		this.selectedProperty = {
			label: modalValue.label,
			alias: modalValue.value,
			isSystem: systemProperties?.some((property) => property.value === modalValue.value) ?? false,
		};

		this.dispatchEvent(new UmbChangeEvent());
	}

	render() {
		return html`<uui-button
			label=${this.localize.term('general_choose')}
			look="placeholder"
			color="default"
			@click=${this.#onClick}></uui-button>`;
	}

	static styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-1);
			}
		`,
	];
}

export default UmbInputContentTypePropertyElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-content-type-property': UmbInputContentTypePropertyElement;
	}
}
