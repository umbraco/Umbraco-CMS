import type { UmbCropModel } from '../../index.js';
import type { UmbMediaCardItemModel } from '../../../../modals/index.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { customElement, html, ifDefined, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import type { UmbUploadableFileModel } from '@umbraco-cms/backoffice/media';

const elementName = 'umb-input-rich-media';
@customElement(elementName)
export class UmbInputRichMediaElement extends UmbInputMediaElement {
	@property({ type: Boolean })
	focalPointEnabled = false;

	@property({ type: Array })
	crops?: Array<UmbCropModel>;

	async #onUploadCompleted(e: CustomEvent) {
		const completed = e.detail?.completed as Array<UmbUploadableFileModel>;
		const uploaded = completed.map((file) => file.unique);

		this.selection = [...this.selection, ...uploaded];
		this.dispatchEvent(new UmbChangeEvent());
	}

	render() {
		return html`${this.#renderDropzone()} ${super.render()}`;
	}

	#renderDropzone() {
		if (this.items && this.items.length >= this.max) return;
		return html`<umb-dropzone @change=${this.#onUploadCompleted}></umb-dropzone>`;
	}

	protected renderItem(item: UmbMediaCardItemModel) {
		return html`
			<uui-card-media
				name=${ifDefined(item.name === null ? undefined : item.name)}
				detail=${ifDefined(item.unique)}
				href="${this.editMediaPath}edit/${item.unique}">
				${item.url
					? html`<img src=${item.url} alt=${item.name} />`
					: html`<umb-icon name=${ifDefined(item.mediaType.icon)}></umb-icon>`}
				${this.renderIsTrashed(item)}
				<uui-action-bar slot="actions">
					<uui-button label="Copy media" look="secondary">
						<uui-icon name="icon-documents"></uui-icon>
					</uui-button>
					<uui-button
						label=${this.localize.term('general_remove')}
						look="secondary"
						@click=${() => this.onRemove(item)}>
						<uui-icon name="icon-trash"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</uui-card-media>
		`;
	}
}

export { UmbInputRichMediaElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbInputRichMediaElement;
	}
}
