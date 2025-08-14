import { umbDataTypeMockDb } from '../../../../../mocks/data/data-type/data-type.db.js';
import type UmbPropertyEditorUIStylesheetPickerElement from './property-editor-ui-stylesheet-picker.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import type { UmbDataTypeDetailModel } from '@umbraco-cms/backoffice/data-type';

import './property-editor-ui-stylesheet-picker.element.js';

const dataTypeData = umbDataTypeMockDb.read('dt-richTextEditor') as unknown as UmbDataTypeDetailModel;

const meta: Meta = {
	title: 'Extension Type/Property Editor UI/Stylesheet Picker',
	component: 'umb-property-editor-ui-stylesheet-picker',
	id: 'umb-property-editor-ui-stylesheet-picker',
	args: {
		value: dataTypeData?.values?.find((x) => x.alias === 'stylesheets')?.value ?? [],
	},
};

export default meta;
type Story = StoryObj<UmbPropertyEditorUIStylesheetPickerElement>;

export const Docs: Story = {};
