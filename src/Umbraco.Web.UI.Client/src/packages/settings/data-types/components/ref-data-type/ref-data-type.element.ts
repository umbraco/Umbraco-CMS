import { UmbDataTypeRepository } from '../../repository/data-type.repository.js';
import { UUIRefNodeElement } from '@umbraco-cms/backoffice/external/uui';
import { html, customElement, property, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

/**
 *  @element umb-ref-data-type
 *  @description - Component for displaying a reference to a Data Type
 *  @extends UUIRefNodeElement
 */
@customElement('umb-ref-data-type')
export class UmbRefDataTypeElement extends UmbElementMixin(UUIRefNodeElement) {
	protected fallbackIcon =
		'<svg xmlns="https://www.w3.org/2000/svg" viewBox="0 0 512 512"><path d="M142.212 397.267l106.052-48.024L398.479 199.03l-26.405-26.442-90.519 90.517-15.843-15.891 90.484-90.486-16.204-16.217-150.246 150.243-47.534 106.513zm74.904-100.739l23.285-23.283 3.353 22.221 22.008 3.124-23.283 23.313-46.176 20.991 20.813-46.366zm257.6-173.71L416.188 64.3l-49.755 49.785 58.504 58.503 49.779-49.77zM357.357 300.227h82.826v116.445H68.929V300.227h88.719v-30.648H38.288v177.733h432.537V269.578H357.357v30.649z"></path></svg>';

	@property({ type: String, attribute: 'data-type-id' })
	public get dataTypeId(): string | undefined {
		return undefined;
	}
	public set dataTypeId(value: string | undefined) {
		this.setDataTypeId(value);
	}

	async setDataTypeId(value: string | undefined) {
		if (value) {
			this.observe(
				(await this.repository.requestById(value)).asObservable(),
				(dataType) => {
					if (dataType) {
						this.name = dataType.name ?? '';
						this.propertyEditorUiAlias = dataType.propertyEditorUiAlias ?? '';
						this.propertyEditorSchemaAlias = dataType.propertyEditorAlias ?? '';
					}
				},
				'dataType'
			);
		} else {
			this.removeControllerByUnique('dataType');
		}
	}

	repository = new UmbDataTypeRepository(this);

	/**
	 * Property Editor UI Alias
	 */
	@state()
	propertyEditorUiAlias = '';

	/**
	 * Property Editor Model Alias
	 */
	@state()
	propertyEditorSchemaAlias = '';

	protected renderDetail() {
		const details: string[] = [];

		if (this.propertyEditorUiAlias !== '') {
			details.push(this.propertyEditorUiAlias);
		} else {
			details.push('Property Editor UI Missing');
		}
		/*
		// TODO: Revisit if its fine to leave this out:
		if (this.propertyEditorSchemaAlias !== '') {
			details.push(this.propertyEditorSchemaAlias);
		} else {
			details.push('Property Editor Model Missing');
		}
		*/

		if (this.detail !== '') {
			details.push(this.detail);
		}
		return html`<small id="detail">${details.join(' | ')}<slot name="detail"></slot></small>`;
	}

	static styles = [
		...UUIRefNodeElement.styles,
		css`
			#detail {
				word-break: break-all;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-data-type': UmbRefDataTypeElement;
	}
}
