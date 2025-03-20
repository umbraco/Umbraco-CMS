import { UmbPropertyEditorUIMultipleTextStringElement } from '../multiple-text-string/property-editor-ui-multiple-text-string.element.js';
import { css, customElement, html, nothing, state, when } from '@umbraco-cms/backoffice/external/lit';
import { formatBytes } from '@umbraco-cms/backoffice/utils';
import { UmbTemporaryFileConfigRepository } from '@umbraco-cms/backoffice/temporary-file';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import type { UmbTemporaryFileConfigurationModel } from '@umbraco-cms/backoffice/temporary-file';

/**
 * @element umb-property-editor-ui-accepted-upload-types
 */
@customElement('umb-property-editor-ui-accepted-upload-types')
export class UmbPropertyEditorUIAcceptedUploadTypesElement
	extends UmbPropertyEditorUIMultipleTextStringElement
	implements UmbPropertyEditorUiElement
{
	#temporaryFileConfigRepository = new UmbTemporaryFileConfigRepository(this);

	@state()
	protected _acceptedTypes: string[] = [];

	@state()
	protected _disallowedTypes: string[] = [];

	@state()
	protected _maxFileSize?: number | null;

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
		const inputElement = this.shadowRoot?.querySelector('umb-input-multiple-text-string');
		inputElement?.addValidator(
			'badInput',
			() => {
				let message = this.localize.term('validation_invalidExtensions');
				if (config.allowedUploadedFileExtensions.length) {
					message += `<br>${this.localize.term('validation_allowedExtensions')} ${config.allowedUploadedFileExtensions.join(', ')}`;
				}
				if (config.disallowedUploadedFilesExtensions.length) {
					message += `<br>${this.localize.term('validation_disallowedExtensions')} ${config.disallowedUploadedFilesExtensions.join(', ')}`;
				}
				return message;
			},
			() => {
				const extensions = inputElement?.items;
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
				<p><umb-localize key="media_noticeExtensionsServerOverride"></umb-localize></p>
				${when(
					this._acceptedTypes.length,
					() => html`
						<p>
							<umb-localize key="validation_allowedExtensions"></umb-localize>
							<strong>${this._acceptedTypes.join(', ')}</strong>
						</p>
					`,
				)}
				${when(
					this._disallowedTypes.length,
					() => html`
						<p>
							<umb-localize key="validation_disallowedExtensions"></umb-localize>
							<strong>${this._disallowedTypes.join(', ')}</strong>
						</p>
					`,
				)}
				${when(
					this._maxFileSize,
					() => html`
						<p>
							${this.localize.term('media_maxFileSize')}
							<strong title="${this.localize.number(this._maxFileSize!)} bytes"
								>${formatBytes(this._maxFileSize!, { decimals: 2 })}</strong
							>.
						</p>
					`,
				)}
			</uui-box>
		`;
	}

	override render() {
		return html`${this.#renderAcceptedTypes()} ${super.render()}`;
	}

	static override readonly styles = [
		css`
			#notice {
				--uui-box-default-padding: var(--uui-size-space-4);
				--uui-box-header-padding: var(--uui-size-space-4);
				--uui-color-divider-standalone: var(--uui-color-warning-standalone);

				border: 1px solid var(--uui-color-divider-standalone);
				background-color: var(--uui-color-warning);
				color: var(--uui-color-warning-contrast);
				margin-bottom: var(--uui-size-layout-1);

				p {
					margin: 0.5rem 0;
				}
			}
		`,
	];
}

export default UmbPropertyEditorUIAcceptedUploadTypesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-accepted-upload-types': UmbPropertyEditorUIAcceptedUploadTypesElement;
	}
}
