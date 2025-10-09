import { UmbPropertyEditorUIMultipleTextStringElement } from '../multiple-text-string/property-editor-ui-multiple-text-string.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
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

	override async connectedCallback() {
		super.connectedCallback();

		await this.#temporaryFileConfigRepository.initialized;
		this.observe(this.#temporaryFileConfigRepository.all(), (config) => {
			if (!config) return;

			this.#addValidators(config);
		});
	}

	#addValidators(config: UmbTemporaryFileConfigurationModel) {
		this.addValidator(
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
				const extensions = this.value;
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
}

export default UmbPropertyEditorUIAcceptedUploadTypesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-accepted-upload-types': UmbPropertyEditorUIAcceptedUploadTypesElement;
	}
}
