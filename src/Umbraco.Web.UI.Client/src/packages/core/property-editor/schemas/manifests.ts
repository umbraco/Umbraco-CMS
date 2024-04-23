import { manifest as imageCropper } from './Umbraco.ImageCropper.js';
import { manifest as markdownEditor } from './Umbraco.MarkdownEditor.js';
import { manifest as mediaPicker } from './Umbraco.MediaPicker.js';
import { manifest as richText } from './Umbraco.RichText.js';

import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestPropertyEditorSchema> = [imageCropper, markdownEditor, mediaPicker, richText];
