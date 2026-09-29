import { UMB_MEMBER_PICKER_PROPERTY_EDITOR_VALUE_TYPE } from '../value-type/constants.js';
import { UMB_MULTIPLE_MEMBER_PICKER_PROPERTY_EDITOR_VALUE_TYPE } from '../../multiple-member-picker/value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.MemberPicker',
		name: 'Member Picker Property Editor Value Summary',
		forValueType: UMB_MEMBER_PICKER_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.js'),
		valueResolver: () => import('./value-summary.js'),
	},
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.MultipleMemberPicker',
		name: 'Multiple Member Picker Property Editor Value Summary',
		forValueType: UMB_MULTIPLE_MEMBER_PICKER_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.js'),
		valueResolver: () => import('./value-summary.js'),
	},
];
