import type { UmbInputMemberElement } from '@umbraco-cms/backoffice/member';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import type { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbTreePickerSource } from '@umbraco-cms/backoffice/components';

@customElement('umb-input-tree')
export class UmbInputTreeElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	private _type: UmbTreePickerSource['type'] = 'content';
	@property()
	public set type(newType: UmbTreePickerSource['type']) {
		const oldType = this._type;
		if (newType?.toLowerCase() !== this._type) {
			this._type = newType?.toLowerCase() as UmbTreePickerSource['type'];
			this.requestUpdate('type', oldType);
		}
	}
	public get type(): UmbTreePickerSource['type'] {
		return this._type;
	}

	@property({ type: String })
	startNodeId?: string;

	@property({ type: Number })
	min = 0;

	@property({ type: Number })
	max = 0;

	private _allowedContentTypeIds: Array<string> = [];
	@property()
	public set allowedContentTypeIds(value: string) {
		this._allowedContentTypeIds = value ? value.split(',') : [];
	}
	public get allowedContentTypeIds(): string {
		return this._allowedContentTypeIds.join(',');
	}

	@property({ type: Boolean })
	showOpenButton?: boolean;

	@property({ type: Boolean })
	ignoreUserStartNodes?: boolean;

	@property()
	public set value(newValue: string) {
		super.value = newValue;
		if (newValue) {
			this.selectedIds = newValue.split(',');
		} else {
			this.selectedIds = [];
		}
	}
	public get value(): string {
		return super.value as string;
	}

	selectedIds: Array<string> = [];

	#onChange(event: CustomEvent) {
		switch (this._type) {
			case 'content':
				this.value = (event.target as UmbInputDocumentElement).selectedIds.join(',');
				break;
			case 'media':
				this.value = (event.target as UmbInputMediaElement).selectedIds.join(',');
				break;
			case 'member':
				this.value = (event.target as UmbInputMemberElement).selectedIds.join(',');
				break;
			default:
				break;
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	constructor() {
		super();
	}

	render() {
		switch (this._type) {
			case 'content':
				return this.#renderContentPicker();
			case 'media':
				return this.#renderMediaPicker();
			case 'member':
				return this.#renderMemberPicker();
			default:
				return html`<p>Type could not be found.</p>`;
		}
	}

	#renderContentPicker() {
		return html`<umb-input-document
			.selectedIds=${this.selectedIds}
			.startNodeId=${this.startNodeId}
			.allowedContentTypeIds=${this._allowedContentTypeIds}
			.min=${this.min}
			.max=${this.max}
			?showOpenButton=${this.showOpenButton}
			?ignoreUserStartNodes=${this.ignoreUserStartNodes}
			@change=${this.#onChange}></umb-input-document>`;
	}

	#renderMediaPicker() {
		return html`<umb-input-media
			.selectedIds=${this.selectedIds}
			.allowedContentTypeIds=${this._allowedContentTypeIds}
			.min=${this.min}
			.max=${this.max}
			?showOpenButton=${this.showOpenButton}
			?ignoreUserStartNodes=${this.ignoreUserStartNodes}
			@change=${this.#onChange}></umb-input-media>`;
	}

	#renderMemberPicker() {
		return html`<umb-input-member
			.selectedIds=${this.selectedIds}
			.allowedContentTypeIds=${this._allowedContentTypeIds}
			.min=${this.min}
			.max=${this.max}
			?showOpenButton=${this.showOpenButton}
			?ignoreUserStartNodes=${this.ignoreUserStartNodes}
			@change=${this.#onChange}></umb-input-member>`;
	}

	static styles = [
		css`
			p {
				margin: 0;
				color: var(--uui-color-border-emphasis);
			}
		`,
	];
}

export default UmbInputTreeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-tree': UmbInputTreeElement;
	}
}
