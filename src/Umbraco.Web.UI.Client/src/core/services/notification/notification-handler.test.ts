import { assert, expect } from '@open-wc/testing';
import { validate as uuidValidate } from 'uuid';
import { UmbNotificationHandler } from './notification-handler';
import type { UmbNotificationDefaultData } from './layouts/default/notification-layout-default.element';
import type { UmbNotificationOptions } from './notification.service';

describe('UCPNotificationHandler', () => {
  let notificationHandler: UmbNotificationHandler;

  beforeEach(async () => {
    const options: UmbNotificationOptions<UmbNotificationDefaultData> = {};
    notificationHandler = new UmbNotificationHandler(options);
  });

  describe('Public API', () => {

    describe('properties', () => {
      it('has a key property', () => {
        expect(notificationHandler).to.have.property('key');
        expect(uuidValidate(notificationHandler.key)).to.be.true;
      });

      it('has an element property', () => {
        expect(notificationHandler).to.have.property('element');
      });

      it('has an color property', () => {
        expect(notificationHandler).to.have.property('color');
      });

      it('has an duration property', () => {
        expect(notificationHandler).to.have.property('duration');
      });

      it('sets a default duration to 6000 ms', () => {
        expect(notificationHandler.duration).to.equal(6000);
      });
    });

    describe('methods', () => {
      it('has a close method', () => {
        expect(notificationHandler).to.have.property('close').that.is.a('function');
      });

      it('has a onClose method', () => {
        expect(notificationHandler).to.have.property('onClose').that.is.a('function');
      });

      it('returns result from close method in onClose promise', () => {
        notificationHandler.close('result value');

        const onClose = notificationHandler.onClose();
        expect(onClose).that.is.a('promise');

        expect(onClose).to.be.not.null;

        if (onClose !== null) {
          return onClose.then(result => {
            expect(result).to.equal('result value');
          });
        }
        
        return assert.fail('onClose should not have returned ´null´');
      });
    });

  });

  describe('Layout', () => {

    describe('Default Layout', () => { 
      let defaultLayoutNotification: UmbNotificationHandler;

      beforeEach(async () => {
        const options: UmbNotificationOptions<UmbNotificationDefaultData> = {
          color: 'positive',
          data: {
            message: 'Notification default layout message',
          },
        };
        defaultLayoutNotification = new UmbNotificationHandler(options);
      });

      it('creates a default layout if a custom element name havnt been specified', () => {      
        expect(defaultLayoutNotification.element.tagName).to.equal('UMB-NOTIFICATION-LAYOUT-DEFAULT');
      });

      it('it sets notificationHandler on custom element', () => {
        expect(defaultLayoutNotification.element.notificationHandler).to.equal(defaultLayoutNotification);
      });
  
      it('it sets data on custom element', () => {
        expect(defaultLayoutNotification.element.data.message).to.equal('Notification default layout message');
      });
    });

    describe('Custom Layout', () => {
      let customLayoutNotification: UmbNotificationHandler;

      beforeEach(async () => {
        const options: UmbNotificationOptions<UmbNotificationDefaultData> = {
          elementName: 'umb-notification-test-element',
          color: 'positive',
          data: {
            message: 'Notification custom layout message',
          },
        };
        customLayoutNotification = new UmbNotificationHandler(options);
      });

      it('creates a custom element', () => {      
        expect(customLayoutNotification.element.tagName).to.equal('UMB-NOTIFICATION-TEST-ELEMENT');
      });
      
      it('it sets notificationHandler on custom element', () => {
        expect(customLayoutNotification.element.notificationHandler).to.equal(customLayoutNotification);
      });
  
      it('it sets data on custom element', () => {
        expect(customLayoutNotification.element.data.message).to.equal('Notification custom layout message');
      });
    });

  });
});