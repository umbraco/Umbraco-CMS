import { manifest as colorEditor } from './color-swatches-editor/manifests.js';
import { manifest as iconPicker } from './icon-picker/manifests.js';
import { manifest as multiUrlPicker } from './multi-url-picker/manifests.js';
import { manifest as numberRange } from './number-range/manifests.js';
import { manifest as orderDirection } from './order-direction/manifests.js';
import { manifest as overlaySize } from './overlay-size/manifests.js';
import { manifest as select } from './select/manifests.js';
import { manifest as uploadField } from './upload-field/manifests.js';
import { manifest as valueType } from './value-type/manifests.js';
import { manifests as checkboxListManifests } from './checkbox-list/manifests.js';
import { manifests as collectionView } from './collection-view/manifests.js';
import { manifests as colorPickerManifests } from './color-picker/manifests.js';
import { manifests as datePickerManifests } from './date-picker/manifests.js';
import { manifests as dropdownManifests } from './dropdown/manifests.js';
import { manifests as eyeDropperManifests } from './eye-dropper/manifests.js';
import { manifests as labelManifests } from './label/manifests.js';
import { manifests as multipleTextStringManifests } from './multiple-text-string/manifests.js';
import { manifests as numbers } from './number/manifests.js';
import { manifests as radioButtonListManifests } from './radio-button-list/manifests.js';
import { manifests as sliderManifests } from './slider/manifests.js';
import { manifests as textareaManifests } from './textarea/manifests.js';
import { manifests as textBoxManifests } from './text-box/manifests.js';
import { manifests as toggleManifests } from './toggle/manifests.js';
import { manifests as treePicker } from './tree-picker/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...checkboxListManifests,
	...collectionView,
	...colorPickerManifests,
	...datePickerManifests,
	...dropdownManifests,
	...eyeDropperManifests,
	...labelManifests,
	...multipleTextStringManifests,
	...numbers,
	...radioButtonListManifests,
	...sliderManifests,
	...textareaManifests,
	...textBoxManifests,
	...toggleManifests,
	...treePicker,
	colorEditor,
	iconPicker,
	multiUrlPicker,
	numberRange,
	orderDirection,
	overlaySize,
	select,
	uploadField,
	valueType,
];
