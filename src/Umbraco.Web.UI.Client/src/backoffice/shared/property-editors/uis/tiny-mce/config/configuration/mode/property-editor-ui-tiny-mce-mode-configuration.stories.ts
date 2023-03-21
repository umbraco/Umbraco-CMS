import { Meta } from '@storybook/web-components';
import { html } from 'lit-html';
import { umbDataTypeData } from '../../../../../../../../core/mocks/data/data-type.data';

import './property-editor-ui-tiny-mce-mode-configuration.element';

const dataTypeData = umbDataTypeData.getByKey('dt-richTextEditor');

export default {
	title: 'Property Editor UIs/Tiny Mce Mode Configuration',
	component: 'umb-property-editor-ui-tiny-mce-mode-configuration',
	id: 'umb-property-editor-ui-tiny-mce-mode-configuration',
} as Meta;

export const AAAOverview = ({ value }: any) =>
	html`<umb-property-editor-ui-tiny-mce-mode-configuration .value=${value}></umb-property-editor-ui-tiny-mce-mode-configuration>`;
AAAOverview.storyName = 'Overview';
AAAOverview.args = {
	value: dataTypeData?.values?.find((x) => x.alias === 'mode')?.value,
};
