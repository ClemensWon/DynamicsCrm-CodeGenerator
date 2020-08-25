﻿using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Yagasoft.CrmCodeGenerator.Models.Mapper;

namespace CrmCodeGenerator.VSPackage.T4
{
    class Processor
    {

        public static string ProcessTemplateCore(string templatePath, string templateContent, Context context, out string extension)
        {
            extension = null;

            // Get the text template service:
            ITextTemplating t4 = Package.GetGlobalService(typeof(STextTemplating)) as ITextTemplating;
            ITextTemplatingSessionHost sessionHost = t4 as ITextTemplatingSessionHost;

            // Create a Session in which to pass parameters:
            sessionHost.Session = sessionHost.CreateSession();
            sessionHost.Session["Context"] = context;

            // string templateContent = System.IO.File.ReadAllText(templatePath);
            Callback cb = new Callback();

            // Process a text template:
            string result = t4.ProcessTemplate(templatePath, templateContent, cb);

            // If there was an output directive in the TemplateFile, then cb.SetFileExtension() will have been called.
            if (!string.IsNullOrWhiteSpace(cb.FileExtension))
            {
                extension = cb.FileExtension;
            }


            // Append any error messages:
            if (cb.ErrorMessages.Count > 0)
            {
                result = cb.ErrorMessages.ToString();
            }
            return result;

        }

        public static string ProcessTemplate(string templatePath, Context context)
        {
            // Get the text template service:
            ITextTemplating t4 = Package.GetGlobalService(typeof(STextTemplating)) as ITextTemplating;
            ITextTemplatingSessionHost sessionHost = t4 as ITextTemplatingSessionHost;

            // Create a Session in which to pass parameters:
            sessionHost.Session = sessionHost.CreateSession();
            sessionHost.Session["Context"] = context;

            string templateContent = System.IO.File.ReadAllText(templatePath);
            Callback cb = new Callback();

            // Process a text template:
            string result = t4.ProcessTemplate(templatePath, templateContent, cb);
            string OutputFullPath;
            if (!string.IsNullOrWhiteSpace(cb.FileExtension))
            {
                // If there was an output directive in the TemplateFile, then cb.SetFileExtension() will have been called.
                OutputFullPath = System.IO.Path.ChangeExtension(templatePath, cb.FileExtension);
            }
            else
            {
                OutputFullPath = System.IO.Path.ChangeExtension(templatePath, ".cs");
            }


            // Write the processed output to file:
            System.IO.File.WriteAllText(OutputFullPath, result, cb.OutputEncoding);

            // Append any error messages:
            if (cb.ErrorMessages.Count > 0)
            {
                System.IO.File.AppendAllLines(OutputFullPath, cb.ErrorMessages.Select(x => x.Message));
            }

            string errroMessage = null;
            if (cb.ErrorMessages.Count > 0)
            {
                errroMessage = "Unable to generate file see " + OutputFullPath + " for more details ";
            }
            return errroMessage;
        }
    }
}
