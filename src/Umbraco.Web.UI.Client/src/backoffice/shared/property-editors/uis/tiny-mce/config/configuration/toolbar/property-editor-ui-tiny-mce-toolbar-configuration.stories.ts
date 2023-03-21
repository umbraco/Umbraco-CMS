import { Meta } from '@storybook/web-components';
import { html } from 'lit-html';
import { umbDataTypeData } from '../../../../../../../../core/mocks/data/data-type.data';

import './property-editor-ui-tiny-mce-toolbar-configuration.element';

const dataTypeData = umbDataTypeData.getByKey('dt-richTextEditor');

export default {
	title: 'Property Editor UIs/Tiny Mce Toolbar Configuration',
	component: 'umb-property-editor-ui-tiny-mce-toolbar-configuration',
	id: 'umb-property-editor-ui-tiny-mce-toolbar-configuration',
} as Meta;

export const AAAOverview = ({ value }: any) =>
	html`<umb-property-editor-ui-tiny-mce-toolbar-configuration .value=${value}></umb-property-editor-ui-tiny-mce-toolbar-configuration>`;

	AAAOverview.storyName = 'Overview';

	AAAOverview.args = {
		value: dataTypeData?.values?.find(x => x.alias === 'toolbar')?.value ?? []
	}