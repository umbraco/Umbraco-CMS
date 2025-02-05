import { manifest as acceptedType } from './accepted-types/manifests.js';
import { manifest as colorEditor } from './color-swatches-editor/manifests.js';
import { manifest as numberRange } from './number-range/manifests.js';
import { manifest as orderDirection } from './order-direction/manifests.js';
import { manifest as overlaySize } from './overlay-size/manifests.js';
import { manifest as select } from './select/manifests.js';
import { manifest as valueType } from './value-type/manifests.js';
import { manifests as checkboxListManifests } from './checkbox-list/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as colorPickerManifests } from './color-picker/manifests.js';
import { manifests as datePickerManifests } from './date-picker/manifests.js';
import { manifests as dropdownManifests } from './dropdown/manifests.js';
import { manifests as eyeDropperManifests } from './eye-dropper/manifests.js';
import { manifests as iconPickerManifests } from './icon-picker/manifests.js';
import { manifests as labelManifests } from './label/manifests.js';
import { manifests as multipleTextStringManifests } from './multiple-text-string/manifests.js';
import { manifests as numberManifests } from './number/manifests.js';
import { manifests as radioButtonListManifests } from './radio-button-list/manifests.js';
import { manifests as sliderManifests } from './slider/manifests.js';
import { manifests as textareaManifests } from './textarea/manifests.js';
import { manifests as textBoxManifests } from './text-box/manifests.js';
import { manifests as toggleManifests } from './toggle/manifests.js';
import { manifests as contentPickerManifests } from './content-picker/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...checkboxListManifests,
	...collectionManifests,
	...colorPickerManifests,
	...datePickerManifests,
	...dropdownManifests,
	...eyeDropperManifests,
	...iconPickerManifests,
	...labelManifests,
	...multipleTextStringManifests,
	...numberManifests,
	...radioButtonListManifests,
	...sliderManifests,
	...textareaManifests,
	...textBoxManifests,
	...toggleManifests,
	...contentPickerManifests,
	acceptedType,
	colorEditor,
	numberRange,
	orderDirection,
	overlaySize,
	select,
	valueType,
];
