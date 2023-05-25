import { Meta, Story } from '@storybook/web-components';
import type { UmbBackofficeElement } from './backoffice.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './backoffice.element.js';

export default {
	title: 'Apps/Backoffice',
	component: 'umb-backoffice',
	id: 'umb-backoffice',
} as Meta;

export const AAAOverview: Story<UmbBackofficeElement> = () => html`<umb-backoffice></umb-backoffice>`;
AAAOverview.storyName = 'Overview';
