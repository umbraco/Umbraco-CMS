import type { UmbBackofficeElement } from './backoffice.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './backoffice.element.js';

export default {
	title: 'Apps/Backoffice',
	component: 'umb-backoffice',
	id: 'umb-backoffice',
} as Meta;

export const AAAOverview: StoryFn<UmbBackofficeElement> = () => html`<umb-backoffice></umb-backoffice>`;
AAAOverview.storyName = 'Overview';
