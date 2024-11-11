import type { UmbContentPickerSource } from '../../types.js';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbReferenceByUniqueAndType } from '@umbraco-cms/backoffice/models';
import type { UmbTreeStartNode } from '@umbraco-cms/backoffice/tree';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

const elementName = 'umb-input-content';
@customElement(elementName)
export class UmbInputContentElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
	UmbLitElement,
) {
	@property()
	public set type(newType: UmbContentPickerSource['type']) {
		const oldType = this.#type;
		if (newType?.toLowerCase() !== this.#type) {
			this.#type = newType?.toLowerCase() as UmbContentPickerSource['type'];
			this.requestUpdate('type', oldType);
		}
	}
	public get type(): UmbContentPickerSource['type'] {
		return this.#type;
	}
	#type: UmbContentPickerSource['type'] = 'content';

	@property({ type: Number })
	min = 0;

	@property({ type: String, attribute: 'min-message' })
	minMessage = 'This field need more items';

	@property({ type: Number })
	max = 0;

	@property({ type: String, attribute: 'max-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	@property({ type: Object, attribute: false })
	startNode?: UmbTreeStartNode;

	@property()
	public set allowedContentTypeIds(value: string) {
		this.#allowedContentTypeIds = value ? value.split(',') : [];
	}
	public get allowedContentTypeIds(): string {
		return this.#allowedContentTypeIds.join(',');
	}
	#allowedContentTypeIds: Array<string> = [];

	@property({ type: Boolean })
	showOpenButton?: boolean;

	@property({ type: Array })
	public set selection(values: Array<UmbReferenceByUniqueAndType>) {
		this.#selection = values?.map((item) => item.unique) ?? [];
	}
	public get selection(): Array<UmbReferenceByUniqueAndType> {
		return this.#selection.map((id) => ({ type: this.#entityTypeLookup[this.#type], unique: id }));
	}

	@property({ type: String })
	public override set value(selectionString: string | undefined) {
		this.#selection = splitStringToArray(selectionString);
	}
	public override get value(): string | undefined {
		return this.#selection.length > 0 ? this.#selection.join(',') : undefined;
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	#entityTypeLookup = { content: 'document', media: 'media', member: 'member' };

	#selection: Array<string> = [];

	override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector(`umb-input-${this.#entityTypeLookup[this.#type]}`)!);
	}

	#onChange(event: CustomEvent & { target: { selection: string[] | undefined } }) {
		this.#selection = event.target.selection ?? [];
		this.value = this.#selection.join(',');
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		switch (this.#type) {
			case 'content':
				return this.#renderDocumentPicker();
			case 'media':
				return this.#renderMediaPicker();
			case 'member':
				return this.#renderMemberPicker();
			default:
				return html`<p>Type could not be found.</p>`;
		}
	}

	#renderDocumentPicker() {
		return html`
			<umb-input-document
				.selection=${this.#selection}
				.startNode=${this.startNode}
				.allowedContentTypeIds=${this.#allowedContentTypeIds}
				.min=${this.min}
				.minMessage=${this.minMessage}
				.max=${this.max}
				.maxMessage=${this.maxMessage}
				?showOpenButton=${this.showOpenButton}
				?readonly=${this.readonly}
				@change=${this.#onChange}></umb-input-document>
		`;
	}

	#renderMediaPicker() {
		return html`
			<umb-input-media
				.selection=${this.#selection}
				.allowedContentTypeIds=${this.#allowedContentTypeIds}
				.min=${this.min}
				.minMessage=${this.minMessage}
				.max=${this.max}
				.maxMessage=${this.maxMessage}
				?showOpenButton=${this.showOpenButton}
				?readonly=${this.readonly}
				@change=${this.#onChange}></umb-input-media>
		`;
	}

	#renderMemberPicker() {
		return html`
			<umb-input-member
				.selection=${this.#selection}
				.allowedContentTypeIds=${this.#allowedContentTypeIds}
				.min=${this.min}
				.minMessage=${this.minMessage}
				.max=${this.max}
				.maxMessage=${this.maxMessage}
				?showOpenButton=${this.showOpenButton}
				?readonly=${this.readonly}
				@change=${this.#onChange}></umb-input-member>
		`;
	}

	static override styles = [
		css`
			p {
				margin: 0;
				color: var(--uui-color-border-emphasis);
			}
		`,
	];
}

export { UmbInputContentElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbInputContentElement;
	}
}
