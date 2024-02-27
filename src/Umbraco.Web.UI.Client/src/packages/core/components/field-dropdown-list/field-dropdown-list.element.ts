import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { css, html, customElement, property, repeat, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { UUIComboboxElement, UUIComboboxEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbMediaTypeDetailRepository } from '@umbraco-cms/backoffice/media-type';
import {
	UMB_DOCUMENT_TYPE_PICKER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT,
	type UmbModalManagerContext,
} from '@umbraco-cms/backoffice/modal';
import { UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';

enum FieldType {
	MEDIA_TYPE = 'media-type',
	DOCUMENT_TYPE = 'document-type',
	SYSTEM = 'system',
}

interface FieldPicker {
	unique: string;
	type: FieldType;
}

@customElement('umb-field-dropdown-list')
export class UmbFieldDropdownListElement extends UmbLitElement {
	@property({ type: String })
	public value = '';

	#documentTypeDetailRepository = new UmbDocumentTypeDetailRepository(this);
	#mediaTypeDetailRepository = new UmbMediaTypeDetailRepository(this);
	#modalManager?: UmbModalManagerContext;

	#unique = new UmbObjectState<{ unique: string; type: FieldType }>({ unique: '', type: FieldType.SYSTEM });
	readonly unique = this.#unique.asObservable();

	@property({ type: Object })
	set documentTypeUnique(value: FieldPicker) {
		this.#unique.setValue(value);
	}
	get documentTypeUnique(): FieldPicker | null | undefined {
		return this.#unique.getValue();
	}

	@state()
	private _documentTypeName?: string;

	@state()
	private _documentTypeIcon?: string;

	@state()
	private _customFields: Array<UmbPropertyTypeModel> = [];

	@state()
	private _type?: FieldType = FieldType.SYSTEM;

	@state()
	private _previewString?: string;

	private _systemFields = [
		{ value: 'sortOrder', name: this.localize.term('general_sort'), group: 'System Fields' },
		{ value: 'updateDate', name: this.localize.term('content_updateDate'), group: 'System Fields' },
		{ value: 'updater', name: this.localize.term('content_updatedBy'), group: 'System Fields' },
		{ value: 'createDate', name: this.localize.term('content_createDate'), group: 'System Fields' },
		{ value: 'owner', name: this.localize.term('content_createBy'), group: 'System Fields' },
		{ value: 'published', name: this.localize.term('content_isPublished'), group: 'System Fields' },
		{ value: 'contentTypeAlias', name: this.localize.term('content_documentType'), group: 'System Fields' },
		{ value: 'email', name: this.localize.term('general_email'), group: 'System Fields' },
		{ value: 'username', name: this.localize.term('general_username'), group: 'System Fields' },
	];

	constructor() {
		super();
		this.observe(this.unique, async (unique) => {
			if (unique) {
				const { data } = await this.#documentTypeDetailRepository.requestByUnique(unique);
				this._customFields = data?.properties ?? [];
				this._documentTypeName = data?.name;
				this._documentTypeIcon = data?.icon;
			} else {
				this._customFields = [];
				this._documentTypeIcon = undefined;
				this._documentTypeName = undefined;
			}
			this.value = '';
			this.dispatchEvent(new UmbChangeEvent());
		});

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (modalManager) => {
			this.#modalManager = modalManager;
		});
	}

	async #changeFieldType() {
		if (!this.#modalManager) return;

		const modalContext = this.#modalManager.open(UMB_DOCUMENT_TYPE_PICKER_MODAL, {
			data: {
				hideTreeRoot: true,
				multiple: false,
			},
			value: {
				selection: this.documentTypeUnique ? [this.documentTypeUnique] : [],
			},
		});

		const modalValue = await modalContext.onSubmit();
		this.documentTypeUnique = modalValue.selection[0] ?? '';

		this._previewString = 'System Fields';
	}

	#onChange(e: UUIComboboxEvent) {
		e.stopPropagation();
		const type = (e.composedPath()[0] as UUIComboboxElement).value as FieldType;
		switch (type) {
			case FieldType.DOCUMENT_TYPE:
				this.#changeFieldType();
				break;
			case FieldType.MEDIA_TYPE:
				console.log(type);
				this.documentTypeUnique = '';
				break;
			default:
				this._previewString = 'System Fields';
				break;
		}
		//const alias = (e.composedPath()[0] as UUIComboboxElement).value as string;
		//this.documentTypeUnique = alias;
		//this.dispatchEvent(new UmbChangeEvent());
	}

	render() {
		return html`
			<div>
				<uui-combobox value="preview">
					<uui-combobox-list @change=${this.#onChange}>
						<uui-combobox-list-option value="preview" style="display:none">
							${this._previewString}
						</uui-combobox-list-option>
						<uui-combobox-list-option value="system"><strong>System Field</strong></uui-combobox-list-option>
						<uui-combobox-list-option value="document-type">
							<strong>Document Type</strong>Pick a document type...
						</uui-combobox-list-option>
						<uui-combobox-list-option value="media-type">
							<strong>Media Type</strong>Pick a media type...
						</uui-combobox-list-option>
					</uui-combobox-list>
				</uui-combobox>
				<uui-combobox>
					<uui-combobox-list>
						<uui-combobox-list-option>Cookies</uui-combobox-list-option>
					</uui-combobox-list>
				</uui-combobox>
			</div>
		`;
	}

	static styles = [
		css`
			:host {
				display: flex;
			}
			strong {
				display: block;
			}
			uui-combobox-list-option {
				padding: calc(var(--uui-size-2, 6px) + 1px);
			}
		`,
	];
}

export default UmbFieldDropdownListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-field-dropdown-list': UmbFieldDropdownListElement;
	}
}
