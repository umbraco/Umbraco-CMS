import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import { UmbNotificationLayoutDefaultElement, UmbNotificationDefaultData } from '.';

export default {
  title: 'API/Notifications/Layouts/Default',
  component: 'umb-notification-layout-default',
  id: 'notification-layout-default',
} as Meta;

const data: UmbNotificationDefaultData = {
  headline: 'Headline',
  message: 'This is a default notification',
};

const Template: Story<UmbNotificationLayoutDefaultElement> = () => html`
  <uui-toast-notification .open=${true}>
    <umb-notification-layout-default .data=${data}></umb-notification-layout-default>
  </uui-toast-notification>
`;

export const Default = Template.bind({});