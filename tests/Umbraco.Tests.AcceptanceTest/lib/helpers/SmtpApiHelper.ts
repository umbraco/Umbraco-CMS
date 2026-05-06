import {ApiHelpers} from "./ApiHelpers";

export class SmtpApiHelper {
  api: ApiHelpers;
  private smtpBaseUrl = 'http://localhost:5000'; // Default smtp4dev URL, can be configured

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async getAllEmails() {
    const response = await this.api.page.request.get(this.smtpBaseUrl + '/api/messages', {
      ignoreHTTPSErrors: true
    });
    return await response.json();
  }

  async deleteAllEmails() {
    const response = await this.api.page.request.delete(this.smtpBaseUrl + '/api/messages/*', {
      ignoreHTTPSErrors: true
    });
    return response.status();
  }

  async findEmailBySubject(subject: string) {
    const emails = await this.getAllEmails();
    const foundEmail = emails.results.find((email: any) =>
      email.subject && email.subject.toLowerCase().includes(subject.toLowerCase())
    );
    return foundEmail || null;
  }

  async doesNotificationEmailWithSubjectExist(actionName: string, contentName: string) {
    const expectedSubject = `Notification about ${actionName} performed on ${contentName}`
    return this.findEmailBySubject(expectedSubject);
  }
}