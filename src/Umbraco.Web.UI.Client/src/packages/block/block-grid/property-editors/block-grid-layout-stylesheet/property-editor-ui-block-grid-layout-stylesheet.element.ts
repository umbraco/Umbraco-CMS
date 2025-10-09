// Needed to disable the import/no-duplicates rule, cause otherwise we do not get the custom element registered:

import type { UmbInputStaticFileElement } from '@umbraco-cms/backoffice/static-file';

import '@umbraco-cms/backoffice/static-file';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-property-editor-ui-block-grid-layout-stylesheet')
export class UmbPropertyEditorUIBlockGridLayoutStylesheetElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	#pickableFilter = (item: any) => item.unique.endsWith('css');
	#singleItemMode = false;
	// TODO: get rid of UmbServerFilePathUniqueSerializer in v.15 [NL]
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	@state()
	private _value?: string | Array<string>;

	@property({ attribute: false })
	public set value(value: string | Array<string> | undefined) {
		if (Array.isArray(value)) {
			this._value = value.map((unique) => this.#serverFilePathUniqueSerializer.toUnique(unique));
		} else if (value) {
			this._value = this.#serverFilePathUniqueSerializer.toUnique(value);
		} else {
			this._value = undefined;
		}
	}
	public get value(): string | Array<string> | undefined {
		if (Array.isArray(this._value)) {
			return this._value.map((unique) => this.#serverFilePathUniqueSerializer.toServerPath(unique) ?? '');
		} else if (this._value) {
			return this.#serverFilePathUniqueSerializer.toServerPath(this._value) ?? '';
		} else {
			return undefined;
		}
	}

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#singleItemMode = config?.getValueByAlias<boolean>('singleItemMode') ?? false;
		const validationLimit = config?.getValueByAlias<UmbNumberRangeValueType>('validationLimit');

		this._limitMin = validationLimit?.min ?? 0;
		this._limitMax = this.#singleItemMode ? 1 : (validationLimit?.max ?? Infinity);
	}

	@state()
	private _limitMin: number = 0;
	@state()
	private _limitMax: number = Infinity;

	private _onChange(event: CustomEvent) {
		if (this.#singleItemMode) {
			this._value = (event.target as UmbInputStaticFileElement).selection[0];
		} else {
			this._value = (event.target as UmbInputStaticFileElement).selection;
		}
		this.dispatchEvent(new UmbChangeEvent());
	}

	// TODO: Implement mandatory?
	override render() {
		return html`
			<umb-input-static-file
				@change=${this._onChange}
				.pickableFilter=${this.#pickableFilter}
				.selection=${this._value ? (Array.isArray(this._value) ? this._value : [this._value]) : []}
				.min=${this._limitMin}
				.max=${this._limitMax}></umb-input-static-file>
			<br />
			<a href="/umbraco/backoffice/css/umbraco-blockgridlayout.css" target="_blank"
				>Link to default layout stylesheet</a
			>
		`;
	}

	static override styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIBlockGridLayoutStylesheetElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-layout-stylesheet': UmbPropertyEditorUIBlockGridLayoutStylesheetElement;
	}
}
