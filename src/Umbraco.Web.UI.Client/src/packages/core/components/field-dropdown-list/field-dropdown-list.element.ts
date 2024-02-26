import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { css, html, customElement, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIComboboxElement, UUIComboboxEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_DOCUMENT_TYPE_PICKER_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-field-dropdown-list')
export class UmbFieldDropdownListElement extends UmbLitElement {
	@property({ type: String })
	public value = '';

	#documentTypeDetailRepository = new UmbDocumentTypeDetailRepository(this);

	#unique = new UmbStringState<string>('');
	readonly unique = this.#unique.asObservable();

	@property({ type: String })
	set documentTypeUnique(value: string) {
		this.#unique.setValue(value);
	}
	get documentTypeUnique(): string | null | undefined {
		return this.#unique.getValue();
	}

	@state()
	private _documentTypeName?: string;

	@state()
	private _documentTypeIcon?: string;

	@state()
	private _customFields: Array<UmbPropertyTypeModel> = [];

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
	}

	#changeFieldType() {
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, async (modalManager) => {
			if (modalManager) {
				const modalContext = modalManager.open(UMB_DOCUMENT_TYPE_PICKER_MODAL, {
					data: {
						hideTreeRoot: true,
						multiple: false,
					},
					value: {
						selection: this.documentTypeUnique ? [this.documentTypeUnique] : [],
					},
				});

				const modalValue = await modalContext?.onSubmit();
				this.documentTypeUnique = modalValue.selection[0] ?? '';
			}
		});
	}

	#onChange(e: UUIComboboxEvent) {
		e.stopPropagation();
		const alias = (e.composedPath()[0] as UUIComboboxElement).value as string;
		this.value = alias;
		this.dispatchEvent(new UmbChangeEvent());
	}

	render() {
		return html`
			<uui-button look="outline" @click=${this.#changeFieldType} compact>
				<uui-icon
					.name=${this.documentTypeUnique
						? this._documentTypeIcon
							? this._documentTypeIcon
							: 'icon-circle-dotted'
						: 'icon-database'}></uui-icon>
				${this.documentTypeUnique ? this._documentTypeName : this.localize.term('formSettings_systemFields')}
			</uui-button>

			<uui-combobox slot="tag" .value=${this.value} @change=${this.#onChange}>
				<uui-combobox-list>
					${this.documentTypeUnique
						? repeat(
								this._customFields,
								(item) => item.id,
								(item) => html`<uui-combobox-list-option .value=${item.alias}>${item.alias}</uui-combobox-list-option>`,
						  )
						: repeat(
								this._systemFields,
								(item) => item.value,
								(item) => html`<uui-combobox-list-option .value=${item.value}>${item.name}</uui-combobox-list-option>`,
						  )}
				</uui-combobox-list>
			</uui-combobox>
		`;
	}

	static styles = [
		css`
			:host {
				display: flex;
			}
			:host > * {
				flex: 1;
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
