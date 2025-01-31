import { UmbPropertyEditorUIMultipleTextStringElement } from '../multiple-text-string/property-editor-ui-multiple-text-string.element.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	UmbTemporaryFileConfigRepository,
	type UmbTemporaryFileConfigurationModel,
} from '@umbraco-cms/backoffice/temporary-file';
import { formatBytes } from '@umbraco-cms/backoffice/utils';

/**
 * @element umb-property-editor-ui-accepted-types
 */
@customElement('umb-property-editor-ui-accepted-types')
export class UmbPropertyEditorUIAcceptedTypesElement
	extends UmbPropertyEditorUIMultipleTextStringElement
	implements UmbPropertyEditorUiElement
{
	@state()
	protected _acceptedTypes: string[] = [];

	@state()
	protected _disallowedTypes: string[] = [];

	@state()
	protected _maxFileSize?: number | null;

	#temporaryFileConfigRepository = new UmbTemporaryFileConfigRepository(this);

	override async connectedCallback() {
		super.connectedCallback();

		await this.#temporaryFileConfigRepository.initialized;
		this.observe(this.#temporaryFileConfigRepository.all(), (config) => {
			if (!config) return;

			this.#addValidators(config);

			this._acceptedTypes = config.allowedUploadedFileExtensions;
			this._disallowedTypes = config.disallowedUploadedFilesExtensions;
			this._maxFileSize = config.maxFileSize ? config.maxFileSize * 1024 : null;
		});
	}

	#addValidators(config: UmbTemporaryFileConfigurationModel) {
		this._inputElement?.addValidator(
			'badInput',
			() => {
				let message = this.localize.term('validation_invalidExtensions');
				if (config.allowedUploadedFileExtensions.length) {
					message += ` ${this.localize.term('validation_allowedExtensions')} ${config.allowedUploadedFileExtensions.join(', ')}`;
				}
				if (config.disallowedUploadedFilesExtensions.length) {
					message += ` ${this.localize.term('validation_disallowedExtensions')} ${config.disallowedUploadedFilesExtensions.join(', ')}`;
				}
				return message;
			},
			() => {
				const extensions = this._inputElement?.items;
				if (!extensions) return false;
				if (
					config.allowedUploadedFileExtensions.length &&
					!config.allowedUploadedFileExtensions.some((ext) => extensions.includes(ext))
				) {
					return true;
				}
				if (config.disallowedUploadedFilesExtensions.some((ext) => extensions.includes(ext))) {
					return true;
				}
				return false;
			},
		);
	}

	#renderAcceptedTypes() {
		if (!this._acceptedTypes.length && !this._disallowedTypes.length && !this._maxFileSize) {
			return nothing;
		}
		return html`
			<uui-box id="notice" headline=${this.localize.term('general_serverConfiguration')}>
				<p>${this.localize.term('media_noticeExtensionsServerOverride')}</p>
				${this._acceptedTypes.length
					? html`<p>
							${this.localize.term('validation_allowedExtensions')} <strong>${this._acceptedTypes.join(', ')}</strong>
						</p>`
					: nothing}
				${this._disallowedTypes.length
					? html`<p>
							${this.localize.term('validation_disallowedExtensions')}
							<strong>${this._disallowedTypes.join(', ')}</strong>
						</p>`
					: nothing}
				${this._maxFileSize
					? html`
							<p>
								${this.localize.term('media_maxFileSize')}
								<strong title="${this.localize.number(this._maxFileSize)} bytes">
									${formatBytes(this._maxFileSize, { decimals: 2 })} </strong
								>.
							</p>
						`
					: nothing}
			</uui-box>
		`;
	}

	override render() {
		return html` ${this.#renderAcceptedTypes()} ${super.render()} `;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			#notice {
				--uui-color-divider-standalone: var(--uui-color-warning-standalone);
				border: 1px solid var(--uui-color-divider-standalone);
				background-color: var(--uui-color-warning);
				color: var(--uui-color-warning-contrast);
				margin-bottom: var(--uui-size-layout-1);
		`,
	];
}

export default UmbPropertyEditorUIAcceptedTypesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-accepted-types': UmbPropertyEditorUIAcceptedTypesElement;
	}
}
