import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent, UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbMediaTypeDetailRepository, UMB_MEDIA_TYPE_PICKER_MODAL } from '@umbraco-cms/backoffice/media-type';
import { UMB_DOCUMENT_TYPE_PICKER_MODAL, UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { UMB_MEMBER_TYPE_PICKER_MODAL, UmbMemberTypeDetailRepository } from '@umbraco-cms/backoffice/member-type';
import { UMB_ITEM_PICKER_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbItemPickerModel, UmbModalToken, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';

export type UmbContentTypePropertyValue = {
	label: string;
	alias: string;
	isSystem: boolean;
};

type UmbInputContentTypePropertyConfigurationItem = {
	item: UmbItemPickerModel;
	pickerModal(): UmbModalToken<any, UmbPickerModalValue>;
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

@customElement('umb-input-collection-content-type-property')
export class UmbInputCollectionContentTypePropertyElement extends UUIFormControlMixin(UmbLitElement, undefined) {
	#configuration: UmbInputContentTypePropertyConfiguration = {
		documentTypes: {
			item: {
				label: this.localize.term('content_documentType'),
				description: this.localize.term('defaultdialogs_selectContentType'),
				value: 'documentTypes',
				icon: 'icon-document',
			},
			pickerModal: () => UMB_DOCUMENT_TYPE_PICKER_MODAL,
			pickableFilter: (docType) => !docType.isElement,
			repository: () => new UmbDocumentTypeDetailRepository(this),
			systemProperties: [
				{
					label: this.localize.term('content_documentType'),
					description: 'contentTypeAlias',
					value: 'contentTypeAlias',
					icon: 'icon-settings',
				},
				{
					label: this.localize.term('content_createDate'),
					description: 'createDate',
					value: 'createDate',
					icon: 'icon-settings',
				},
				{
					label: this.localize.term('content_createBy'),
					description: 'creator',
					value: 'creator',
					icon: 'icon-settings',
				},
				{
					label: this.localize.term('content_isPublished'),
					description: 'published',
					value: 'published',
					icon: 'icon-settings',
				},
				{
					label: this.localize.term('general_sort'),
					description: 'sortOrder',
					value: 'sortOrder',
					icon: 'icon-settings',
				},
				{
					label: this.localize.term('content_updateDate'),
					description: 'updateDate',
					value: 'updateDate',
					icon: 'icon-settings',
				},
				{
					label: this.localize.term('content_updatedBy'),
					description: 'updater',
					value: 'updater',
					icon: 'icon-settings',
				},
			],
		},
		elementTypes: {
			item: {
				label: this.localize.term('create_elementType'),
				description: this.localize.term('content_nestedContentSelectElementTypeModalTitle'),
				value: 'elementTypes',
				icon: 'icon-plugin',
			},
			pickerModal: () => UMB_DOCUMENT_TYPE_PICKER_MODAL,
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
				icon: 'icon-picture',
			},
			pickerModal: () => UMB_MEDIA_TYPE_PICKER_MODAL,
			repository: () => new UmbMediaTypeDetailRepository(this),
			systemProperties: [
				{
					label: this.localize.term('content_documentType'),
					description: 'contentTypeAlias',
					value: 'contentTypeAlias',
					icon: 'icon-settings',
				},
				{
					label: this.localize.term('content_createDate'),
					description: 'createDate',
					value: 'createDate',
					icon: 'icon-settings',
				},
				{
					label: this.localize.term('content_createBy'),
					description: 'creator',
					value: 'creator',
					icon: 'icon-settings',
				},
				{
					label: this.localize.term('general_sort'),
					description: 'sortOrder',
					value: 'sortOrder',
					icon: 'icon-settings',
				},
				{
					label: this.localize.term('content_updateDate'),
					description: 'updateDate',
					value: 'updateDate',
					icon: 'icon-settings',
				},
				{
					label: this.localize.term('content_updatedBy'),
					description: 'updater',
					value: 'updater',
					icon: 'icon-settings',
				},
			],
		},
		memberTypes: {
			item: {
				label: this.localize.term('content_membertype'),
				description: this.localize.term('defaultdialogs_selectMemberType'),
				value: 'memberTypes',
				icon: 'icon-user',
			},
			pickerModal: () => UMB_MEMBER_TYPE_PICKER_MODAL,
			repository: () => new UmbMemberTypeDetailRepository(this),
			systemProperties: [
				{
					label: this.localize.term('content_documentType'),
					description: 'contentTypeAlias',
					value: 'contentTypeAlias',
					icon: 'icon-settings',
				},
				{
					label: this.localize.term('content_createDate'),
					description: 'createDate',
					value: 'createDate',
					icon: 'icon-settings',
				},
				{ label: this.localize.term('general_email'), description: 'email', value: 'email', icon: 'icon-settings' },
				{
					label: this.localize.term('content_updateDate'),
					description: 'updateDate',
					value: 'updateDate',
					icon: 'icon-settings',
				},
				{
					label: this.localize.term('general_username'),
					description: 'username',
					value: 'username',
					icon: 'icon-settings',
				},
			],
		},
	};

	#modalManager?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;

	protected override getFormElement() {
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

		const pickerModal = config.pickerModal();

		const pickerContext = this.#modalManager?.open(this, pickerModal, {
			data: {
				hideTreeRoot: true,
				multiple: false,
				pickableFilter: config.pickableFilter,
			},
		});

		// NOTE: We listen for the selection change event to submit the picker.
		// This is to enforce a single selection and progress to the next modal.
		pickerContext?.addEventListener(UmbSelectionChangeEvent.TYPE, pickerContext.submit);

		const pickerValue = await pickerContext?.onSubmit();

		const selectedItems = pickerValue?.selection ?? [];
		if (selectedItems.length === 0) return;

		const repository = config.repository();
		const { data } = await repository.requestByUnique(selectedItems[0] ?? '');
		if (!data) return;

		this.#openPropertyPicker(data, config);
	}

	async #openPropertyPicker(contentType: UmbContentTypeModel, config: UmbInputContentTypePropertyConfigurationItem) {
		if (!contentType) return;
		if (!this.#modalManager) return;

		const properties: Array<UmbItemPickerModel> =
			contentType?.properties.map((property) => ({
				label: property.name,
				value: property.alias,
				description: property.alias,
				icon: config.item.icon,
			})) ?? [];

		const items = [...(config.systemProperties ?? []), ...properties];

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
			isSystem: config.systemProperties?.some((property) => property.value === modalValue.value) ?? false,
		};

		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<uui-button
			label=${this.localize.term('general_choose')}
			look="placeholder"
			color="default"
			@click=${this.#onClick}></uui-button>`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-1);
			}
		`,
	];
}

export default UmbInputCollectionContentTypePropertyElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-collection-content-type-property': UmbInputCollectionContentTypePropertyElement;
	}
}
