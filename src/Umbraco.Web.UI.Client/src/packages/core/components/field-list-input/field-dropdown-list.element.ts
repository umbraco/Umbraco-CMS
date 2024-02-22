import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { css, html, customElement, property, repeat, state, query } from '@umbraco-cms/backoffice/external/lit';
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

	@query('uui-combobox')
	private _combobox!: UUIComboboxElement;

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
				// TODO: Make as mode for the Picker Modal, so the click to select immediately submits the modal(And in that mode we do not want to see a Submit button).
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
			<uui-ref-node
				@open=${this.#changeFieldType}
				.name=${this._documentTypeName ?? 'System Field'}
				@change=${this.#onChange}>
				<uui-icon name=${this._documentTypeIcon ?? 'icon-database'} slot="icon"></uui-icon>
				<uui-combobox slot="tag" .value=${this.value}>
					<uui-combobox-list>
						${this.documentTypeUnique
							? repeat(
									this._customFields,
									(item) => item.id,
									(item) =>
										html`<uui-combobox-list-option .value=${item.alias}>${item.alias}</uui-combobox-list-option>`,
							  )
							: repeat(
									this._systemFields,
									(item) => item.value,
									(item) =>
										html`<uui-combobox-list-option .value=${item.value}>${item.name}</uui-combobox-list-option>`,
							  )}
					</uui-combobox-list>
				</uui-combobox>
			</uui-ref-node>
		`;
	}

	static styles = [
		css`
			uui-ref-node {
				width: auto;
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
